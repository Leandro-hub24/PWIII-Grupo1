using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RuteoYa.Entidades;

/// <summary>
/// Permite correr "dotnet ef migrations add ..." con RuteoYa.Entidades como proyecto de inicio.
/// La cadena real la pone OnConfiguring / appsettings de cada proyecto.
/// </summary>
public class RuteoYaDbContextFactory : IDesignTimeDbContextFactory<RuteoYaDbContext>
{
    public RuteoYaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<RuteoYaDbContext>().Options;
        return new RuteoYaDbContext(options);
    }
}
