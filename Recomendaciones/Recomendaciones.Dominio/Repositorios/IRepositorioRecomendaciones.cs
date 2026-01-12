using Recomendaciones.Dominio.Entidades;

namespace Recomendaciones.Dominio.Repositorios;

public interface IRepositorioRecomendaciones
{
    // Afinidad
    Task<AfinidadUsuario?> ObtenerAfinidadAsync(Guid usuarioId, string categoria);
    Task<IEnumerable<AfinidadUsuario>> ObtenerAfinidadesPorUsuarioAsync(Guid usuarioId);
    Task AgregarAfinidadAsync(AfinidadUsuario afinidad);
    Task ActualizarAfinidadAsync(AfinidadUsuario afinidad);

    // Eventos
    Task<EventoProyeccion?> ObtenerEventoAsync(Guid eventoId);
    Task AgregarEventoAsync(EventoProyeccion evento);
    Task ActualizarEventoAsync(EventoProyeccion evento);
    Task<IEnumerable<EventoProyeccion>> ObtenerEventosFuturosAsync();
    Task<IEnumerable<EventoProyeccion>> ObtenerEventosPorCategoriasAsync(IEnumerable<string> categorias);
}
