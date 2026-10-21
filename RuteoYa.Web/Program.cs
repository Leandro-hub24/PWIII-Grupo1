using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
using var db = new RuteoYaDbContext();
db.Database.Migrate();

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<RuteoYaDbContext>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
