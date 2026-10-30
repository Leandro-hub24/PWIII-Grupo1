using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades;
using RuteoYa.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<RuteoYaDbContext>();

// Servicios propios de Web (orquestadores, servicios HTTP que consumen API)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001";
builder.Services.AddWebServices(apiBaseUrl);


// Add services to the container.
builder.Services.AddScoped<IUsuariosService, UsuariosService>();


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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
