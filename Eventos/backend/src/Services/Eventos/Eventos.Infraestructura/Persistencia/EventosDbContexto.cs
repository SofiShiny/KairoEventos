using Microsoft.EntityFrameworkCore;
using Eventos.Dominio.Entidades;
using Eventos.Infraestructura.Persistencia.Configuraciones;
using BloquesConstruccion.Dominio;

namespace Eventos.Infraestructura.Persistencia;

public class EventosDbContext : DbContext
{
    public EventosDbContext(DbContextOptions<EventosDbContext> options) : base(options)
    {
    }

    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Asistente> Asistentes => Set<Asistente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<EventoDominio>();

        modelBuilder.ApplyConfiguration(new EventoConfiguration());
        modelBuilder.ApplyConfiguration(new AsistenteConfiguration());
    }
}
