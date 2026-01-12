using Microsoft.EntityFrameworkCore;
using Recomendaciones.Dominio.Entidades;

namespace Recomendaciones.Infraestructura.Persistencia;

public class RecomendacionesDbContext : DbContext
{
    public RecomendacionesDbContext(DbContextOptions<RecomendacionesDbContext> options) : base(options) { }

    public DbSet<AfinidadUsuario> AfinidadesUsuarios => Set<AfinidadUsuario>();
    public DbSet<EventoProyeccion> EventosProyecciones => Set<EventoProyeccion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AfinidadUsuario>(entity =>
        {
            entity.ToTable("afinidades_usuario");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UsuarioId, e.Categoria }).IsUnique();
        });

        modelBuilder.Entity<EventoProyeccion>(entity =>
        {
            entity.ToTable("eventos_proyecciones");
            entity.HasKey(e => e.EventoId);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Categoria).IsRequired().HasMaxLength(50);
        });
    }
}
