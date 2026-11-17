using Microsoft.EntityFrameworkCore;
using RuteoYa.Entidades.Entities;
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
            @"Server=(localdb)\MSSQLLocalDB;Database=RuteoYa;Trusted_Connection=True;TrustServerCertificate=True");
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

            // Configurar precisión para que coincida con migraciones
            entity.Property(e => e.OrigenLat).HasPrecision(18, 2);
            entity.Property(e => e.OrigenLng).HasPrecision(18, 2);
            entity.Property(e => e.DestinoLat).HasPrecision(18, 2);
            entity.Property(e => e.DestinoLng).HasPrecision(18, 2);
            entity.Property(e => e.PesoKg).HasPrecision(18, 2);
            entity.Property(e => e.Monto).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Viaje>(entity =>
        {
            entity.HasOne(e => e.Usuario)
                  .WithMany(u => u.Viajes)
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Configurar precisión para que coincida con migraciones
            entity.Property(v => v.OrigenLat).HasPrecision(18, 2);
            entity.Property(v => v.OrigenLng).HasPrecision(18, 2);
            entity.Property(v => v.DestinoLat).HasPrecision(18, 2);
            entity.Property(v => v.DestinoLng).HasPrecision(18, 2);
            entity.Property(v => v.PesoMaxKg).HasPrecision(18, 2);
            entity.Property(v => v.DesvioMaxKm).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Parada>(entity =>
        {
            // Configurar precisión para que coincida con migraciones
            entity.Property(p => p.Lat).HasPrecision(18, 2);
            entity.Property(p => p.Lng).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Asignacion>(entity =>
        {
            // Configurar precisión para que coincida con migraciones
            entity.Property(a => a.DesvioKm).HasPrecision(18, 2);
            entity.Property(a => a.Ganancia).HasPrecision(18, 2);
        });

        // Seed de datos - Usuario de prueba
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nombre = "Admin",
                Apellido = "Sistema",
                Email = "admin@ruteoya.com",
                // Password: Admin123!
                PasswordHash = "$2a$11$xKvLhR4QZJvKZN8Y5Y8wWeF.0KK/jZqR6HjF4V5hFhqE3YHxMYGhK",
                CreadoEn = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }

}
