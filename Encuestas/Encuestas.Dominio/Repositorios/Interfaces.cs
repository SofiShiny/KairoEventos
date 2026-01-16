using Encuestas.Dominio.Entidades;

namespace Encuestas.Dominio.Repositorios;

public interface IRepositorioEncuestas
{
    Task<Encuesta?> ObtenerPorIdAsync(Guid id);
    Task<Encuesta?> ObtenerPorEventoIdAsync(Guid eventoId);
    Task AgregarEncuestaAsync(Encuesta encuesta);
    Task ActualizarEncuestaAsync(Encuesta encuesta);

    Task<bool> UsuarioYaRespondioAsync(Guid encuestaId, Guid usuarioId);
    Task GuardarRespuestaAsync(RespuestaUsuario respuesta);
    Task<IEnumerable<RespuestaUsuario>> ObtenerRespuestasAsync(Guid encuestaId);
}

public interface IVerificadorAsistencia
{
    Task<bool> VerificarAsistenciaAsync(Guid usuarioId, Guid eventoId);
}
