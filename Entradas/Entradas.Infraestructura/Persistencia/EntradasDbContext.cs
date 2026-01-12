using Microsoft.EntityFrameworkCore;
using Entradas.Dominio.Entidades;
using Entradas.Infraestructura.Persistencia.Configuraciones;

namespace Entradas.Infraestructura.Persistencia;

/// <summary>
/// Contexto de base de datos para el microservicio de entradas
/// </summary>
public class EntradasDbContext : DbContext
{
    public EntradasDbContext(DbContextOptions<EntradasDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet para las entradas
    /// </summary>
    public DbSet<Entrada> Entradas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Aplicar configuraciones de entidades
        modelBuilder.ApplyConfiguration(new EntradaConfiguration());
        
        // Configurar esquema por defecto
        modelBuilder.HasDefaultSchema("entradas");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Configuraciones adicionales para desarrollo
        if (!optionsBuilder.IsConfigured)
        {
            // Esta configuración solo se usa si no se ha configurado externamente
            optionsBuilder.UseNpgsql("Host=localhost;Database=entradas_dev;Username=postgres;Password=postgres");
        }
    }

    /// <summary>
    /// Configura el comportamiento de SaveChanges para actualizar automáticamente las fechas
    /// </summary>
    public override int SaveChanges()
    {
        ActualizarFechasEntidades();
        return base.SaveChanges();
    }

    /// <summary>
    /// Configura el comportamiento de SaveChangesAsync para actualizar automáticamente las fechas
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ActualizarFechasEntidades();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Actualiza las fechas de creación y modificación de las entidades
    /// </summary>
    private void ActualizarFechasEntidades()
    {
        var entradas = ChangeTracker.Entries<EntidadBase>();
        var ahora = DateTime.UtcNow;

        foreach (var entrada in entradas)
        {
            switch (entrada.State)
            {
                case EntityState.Added:
                    entrada.Property(e => e.FechaCreacion).CurrentValue = ahora;
                    entrada.Property(e => e.FechaActualizacion).CurrentValue = ahora;
                    break;
                
                case EntityState.Modified:
                    entrada.Property(e => e.FechaActualizacion).CurrentValue = ahora;
                    break;
            }
        }
    }
}