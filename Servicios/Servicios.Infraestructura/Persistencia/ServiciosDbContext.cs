using Microsoft.EntityFrameworkCore;
using Servicios.Dominio.Entidades;

namespace Servicios.Infraestructura.Persistencia;

public class ServiciosDbContext : DbContext
{
    public ServiciosDbContext(DbContextOptions<ServiciosDbContext> options) : base(options) { }

    public DbSet<ServicioGlobal> ServiciosGlobales => Set<ServicioGlobal>();
    public DbSet<ProveedorServicio> ProveedoresServicios => Set<ProveedorServicio>();
    public DbSet<ReservaServicio> ReservasServicios => Set<ReservaServicio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServicioGlobal>(entity =>
        {
            entity.ToTable("servicios_globales");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Precio).IsRequired();
            
            entity.HasMany(e => e.Proveedores)
                  .WithOne()
                  .HasForeignKey(p => p.ServicioId);
        });

        modelBuilder.Entity<ProveedorServicio>(entity =>
        {
            entity.ToTable("proveedores_servicios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreProveedor).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExternalId).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<ReservaServicio>(entity =>
        {
            entity.ToTable("reservas_servicios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Estado).HasConversion<string>();
            entity.Property(e => e.OrdenEntradaId).HasColumnName("orden_entrada_id");
        });
    }
}
