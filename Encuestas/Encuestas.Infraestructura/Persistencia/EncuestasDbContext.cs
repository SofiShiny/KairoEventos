using Microsoft.EntityFrameworkCore;
using Encuestas.Dominio.Entidades;

namespace Encuestas.Infraestructura.Persistencia;

public class EncuestasDbContext : DbContext
{
    public EncuestasDbContext(DbContextOptions<EncuestasDbContext> options) : base(options) { }

    public DbSet<Encuesta> Encuestas => Set<Encuesta>();
    public DbSet<Pregunta> Preguntas => Set<Pregunta>();
    public DbSet<RespuestaUsuario> RespuestasUsuarios => Set<RespuestaUsuario>();
    public DbSet<ValorRespuesta> ValoresRespuestas => Set<ValorRespuesta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Encuesta>(entity =>
        {
            entity.ToTable("encuestas");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Preguntas).WithOne().HasForeignKey(p => p.EncuestaId);
        });

        modelBuilder.Entity<Pregunta>(entity =>
        {
            entity.ToTable("preguntas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipo).HasConversion<string>();
        });

        modelBuilder.Entity<RespuestaUsuario>(entity =>
        {
            entity.ToTable("respuestas_usuarios");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Valores).WithOne().HasForeignKey(v => v.RespuestaUsuarioId);
        });

        modelBuilder.Entity<ValorRespuesta>(entity =>
        {
            entity.ToTable("valores_respuestas");
            entity.HasKey(e => e.Id);
        });
    }
}
