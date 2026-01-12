using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;
using Encuestas.Dominio.Entidades;
using Encuestas.Dominio.Repositorios;
using Encuestas.Aplicacion.Eventos;

namespace Encuestas.Aplicacion.Comandos;

public record RespuestaDto(Guid PreguntaId, string Valor);

public record ResponderEncuestaCommand(
    Guid EncuestaId,
    Guid UsuarioId,
    List<RespuestaDto> Respuestas
) : IRequest<Guid>;

public class ResponderEncuestaCommandHandler : IRequestHandler<ResponderEncuestaCommand, Guid>
{
    private readonly IRepositorioEncuestas _repositorio;
    private readonly IVerificadorAsistencia _verificadorAsistencia;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ResponderEncuestaCommandHandler> _logger;

    public ResponderEncuestaCommandHandler(
        IRepositorioEncuestas repositorio,
        IVerificadorAsistencia verificadorAsistencia,
        IPublishEndpoint publishEndpoint,
        ILogger<ResponderEncuestaCommandHandler> logger)
    {
        _repositorio = repositorio;
        _verificadorAsistencia = verificadorAsistencia;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Guid> Handle(ResponderEncuestaCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar existencia y estado de la encuesta
        var encuesta = await _repositorio.ObtenerPorIdAsync(request.EncuestaId);
        if (encuesta == null || !encuesta.Publicada)
        {
            throw new InvalidOperationException("La encuesta no existe o no está publicada");
        }

        // 2. Validar unicidad (una respuesta por usuario)
        var yaRespondio = await _repositorio.UsuarioYaRespondioAsync(request.EncuestaId, request.UsuarioId);
        if (yaRespondio)
        {
            throw new InvalidOperationException("El usuario ya ha respondido a esta encuesta");
        }

        // 3. Validar asistencia (Llamada HTTP a Entradas.API para estado 'Usada')
        _logger.LogInformation("Verificando asistencia de usuario {UsuarioId} al evento {EventoId}", request.UsuarioId, encuesta.EventoId);
        var asistio = await _verificadorAsistencia.VerificarAsistenciaAsync(request.UsuarioId, encuesta.EventoId);
        
        if (!asistio)
        {
            _logger.LogWarning("Intento de encuesta rechazado: El usuario {UsuarioId} no registró asistencia al evento {EventoId}", request.UsuarioId, encuesta.EventoId);
            throw new UnauthorizedAccessException("Solo los usuarios que asistieron al evento pueden completar la encuesta");
        }

        // 4. Guardar respuesta
        var respuesta = new RespuestaUsuario(request.EncuestaId, request.UsuarioId);
        foreach (var r in request.Respuestas)
        {
            respuesta.AgregarRespuesta(r.PreguntaId, r.Valor);
        }

        await _repositorio.GuardarRespuestaAsync(respuesta);

        // 5. Publicar evento (Opcional para reportes/auditoria)
        await _publishEndpoint.Publish(new EncuestaCompletadaEvento(
            respuesta.Id,
            encuesta.Id,
            request.UsuarioId,
            encuesta.EventoId,
            respuesta.Fecha
        ), cancellationToken);

        _logger.LogInformation("Encuesta {EncuestaId} completada exitosamente por usuario {UsuarioId}", request.EncuestaId, request.UsuarioId);

        return respuesta.Id;
    }
}

public record PublicarEncuestaCommand(Guid EncuestaId) : IRequest;

public class PublicarEncuestaCommandHandler : IRequestHandler<PublicarEncuestaCommand>
{
    private readonly IRepositorioEncuestas _repositorio;

    public PublicarEncuestaCommandHandler(IRepositorioEncuestas repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task Handle(PublicarEncuestaCommand request, CancellationToken cancellationToken)
    {
        var encuesta = await _repositorio.ObtenerPorIdAsync(request.EncuestaId);
        if (encuesta == null) throw new KeyNotFoundException();
        
        encuesta.Publicar();
        await _repositorio.ActualizarEncuestaAsync(encuesta);
    }
}
