using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades.Entities;
using System;
using System.Collections.Generic;
using System.Text;
namespace RuteoYa.Entidades;

public class RuteoYaDbContext : DbContext
{
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Envio> Envios { get; set; }
    public DbSet<Viaje> Viajes { get; set; }
    public DbSet<Asignacion> Asignaciones { get; set; }
    public DbSet<EnvioEstadoHistorial> EnvioEstadoHistoriales { get; set; }
    public DbSet<Parada> Paradas { get; set; }
    public DbSet<ViajeDia> ViajeDias { get; set; }

    public RuteoYaDbContext(DbContextOptions<RuteoYaDbContext> options) : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=.\SQLEXPRESS;Database=RuteoYa;Trusted_Connection=True;TrustServerCertificate=True");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

       
        modelBuilder.Entity<Envio>(entity =>
        {
            entity.HasOne(e => e.Usuario)
                  .WithMany(u => u.Envios)
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Viaje>(entity =>
        {
            entity.HasOne(e => e.Usuario)
                  .WithMany(u => u.Viajes)
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

}
