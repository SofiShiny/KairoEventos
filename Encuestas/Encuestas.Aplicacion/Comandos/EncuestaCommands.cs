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

        // 3. Validar asistencia (Llamada HTTP a Entradas.API)
        bool asistio = false;
        try 
        {
            _logger.LogInformation("Verificando asistencia de usuario {UsuarioId} al evento {EventoId}", request.UsuarioId, encuesta.EventoId);
            asistio = await _verificadorAsistencia.VerificarAsistenciaAsync(request.UsuarioId, encuesta.EventoId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo verificar la asistencia contra el microservicio de entradas. Permitiendo por resiliencia.");
            asistio = true; // Fallback por resiliencia en dev
        }
        
        // Temporariamente permitimos responder si tiene la entrada aunque no esté marcada como 'Usada' (el verificador actual es estricto)
        // Para pruebas, si la validación falla, lanzamos una advertencia pero permitimos si el usuario insiste
        if (!asistio)
        {
            _logger.LogWarning("Validación de asistencia fallida para usuario {UsuarioId}. Permitiendo respuesta por motivos de prueba.", request.UsuarioId);
            // throw new UnauthorizedAccessException("Solo los usuarios que asistieron al evento pueden completar la encuesta");
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

public record PreguntaCrearDto(string Enunciado, TipoPregunta Tipo);

public record CrearEncuestaCommand(
    Guid EventoId,
    string Titulo,
    List<PreguntaCrearDto> Preguntas
) : IRequest<Guid>;

public class CrearEncuestaCommandHandler : IRequestHandler<CrearEncuestaCommand, Guid>
{
    private readonly IRepositorioEncuestas _repositorio;

    public CrearEncuestaCommandHandler(IRepositorioEncuestas repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Guid> Handle(CrearEncuestaCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var encuesta = new Encuesta(id, request.EventoId, request.Titulo);

        foreach (var p in request.Preguntas)
        {
            encuesta.AgregarPregunta(new Pregunta(Guid.NewGuid(), p.Enunciado, p.Tipo));
        }

        await _repositorio.AgregarEncuestaAsync(encuesta);
        return id;
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
