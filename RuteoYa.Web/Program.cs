using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades;
using RuteoYa.Web;
using RuteoYa.Web.Common;
using RuteoYa.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<RuteoYaDbContext>();

// Servicios propios de Web (orquestadores, servicios HTTP que consumen API)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001";
builder.Services.AddWebServices(apiBaseUrl);


// Add services to the container.
builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEnviosApiService, EnviosApiService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // Dónde redirigir si no está autenticado
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // Tiempo de vida de la cookie local

        // Configuraciones de seguridad estrictas de la cookie
        options.Cookie.Name = "MiApp.AuthCookie";
        options.Cookie.HttpOnly = true;  // Inaccesible para JavaScript (Protección contra XSS)
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Usa HTTPS si la petición lo es
        options.Cookie.SameSite = SameSiteMode.Lax; // O Strict según tus necesidades de redirección
    });

// Necesario para que el DelegatingHandler pueda leer la sesión del usuario
builder.Services.AddHttpContextAccessor();

// Registrar nuestro handler personalizado
builder.Services.AddTransient<TokenDelegatingHandler>();

builder.Services.AddHttpClient("MiApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
    .AddHttpMessageHandler<TokenDelegatingHandler>();


var app = builder.Build();

// Migración automática al iniciar (solo para dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RuteoYaDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
