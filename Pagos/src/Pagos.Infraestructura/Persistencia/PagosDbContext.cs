using Microsoft.EntityFrameworkCore;
using Pagos.Dominio.Entidades;

namespace Pagos.Infraestructura.Persistencia;

public class PagosDbContext : DbContext
{
    public PagosDbContext(DbContextOptions<PagosDbContext> options) : base(options) { }

    public DbSet<Transaccion> Transacciones => Set<Transaccion>();
    public DbSet<Cupon> Cupones => Set<Cupon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaccion>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Monto).HasPrecision(18, 2);
            b.Property(x => x.TarjetaMascara).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<Cupon>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Codigo).IsRequired().HasMaxLength(20);
            b.Property(x => x.PorcentajeDescuento).HasPrecision(5, 2); // Permite hasta 999.99%
            b.HasIndex(x => x.Codigo).IsUnique();
            b.HasIndex(x => x.EventoId);
            b.HasIndex(x => x.Estado);
        });
    }
}
