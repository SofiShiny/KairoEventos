using Microsoft.EntityFrameworkCore;
using Marketing.Dominio.Entidades;

namespace Marketing.Infraestructura.Persistencia;

public class MarketingDbContext : DbContext
{
    public MarketingDbContext(DbContextOptions<MarketingDbContext> options) : base(options)
    {
    }

    public DbSet<Cupon> Cupones => Set<Cupon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cupon>(entity =>
        {
            entity.ToTable("Cupones");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Codigo)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.Codigo)
                .IsUnique();

            entity.Property(e => e.Valor)
                .HasPrecision(18, 2);

            entity.Property(e => e.TipoDescuento)
                .HasConversion<string>();

            entity.Property(e => e.Estado)
                .HasConversion<string>();
        });
    }
}
