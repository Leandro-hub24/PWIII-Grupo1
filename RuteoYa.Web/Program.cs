using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades;
using RuteoYa.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Dónde redirigir si no está autenticado
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // Tiempo de vida de la cookie local

        // Configuraciones de seguridad estrictas de la cookie
        options.Cookie.Name = "MiApp.AuthCookie";
        options.Cookie.HttpOnly = true;  // Inaccesible para JavaScript (Protección contra XSS)
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Usa HTTPS si la petición lo es
        options.Cookie.SameSite = SameSiteMode.Lax; // O Strict según tus necesidades de redirección
    });

builder.Services.AddHttpClient("MiApi", client =>
{
    // Reemplaza esto con el puerto y URL real donde corre tu Web API
    client.BaseAddress = new Uri("https://localhost:7019/");
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

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
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
