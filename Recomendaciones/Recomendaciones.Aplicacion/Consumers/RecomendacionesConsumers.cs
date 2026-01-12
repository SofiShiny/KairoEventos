using MassTransit;
using Recomendaciones.Dominio.Entidades;
using Recomendaciones.Dominio.Repositorios;
using Recomendaciones.Aplicacion.Eventos;
using Microsoft.Extensions.Logging;

namespace Recomendaciones.Aplicacion.Consumers;

public class EntradaCompradaConsumer : IConsumer<EntradaCompradaEvento>
{
    private readonly IRepositorioRecomendaciones _repositorio;
    private readonly ILogger<EntradaCompradaConsumer> _logger;

    public EntradaCompradaConsumer(IRepositorioRecomendaciones repositorio, ILogger<EntradaCompradaConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EntradaCompradaEvento> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Procesando EntradaComprada para usuario {UsuarioId}, Categoria {Categoria}", msg.UsuarioId, msg.Categoria);

        var afinidad = await _repositorio.ObtenerAfinidadAsync(msg.UsuarioId, msg.Categoria);
        if (afinidad == null)
        {
            afinidad = new AfinidadUsuario(msg.UsuarioId, msg.Categoria);
            await _repositorio.AgregarAfinidadAsync(afinidad);
        }

        afinidad.SumarPuntos(10);
        await _repositorio.ActualizarAfinidadAsync(afinidad);
    }
}

public class EventoCreadoConsumer : IConsumer<EventoCreadoEvento>
{
    private readonly IRepositorioRecomendaciones _repositorio;
    private readonly ILogger<EventoCreadoConsumer> _logger;

    public EventoCreadoConsumer(IRepositorioRecomendaciones repositorio, ILogger<EventoCreadoConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EventoCreadoEvento> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Proyectando nuevo evento {EventoId}: {Titulo}", msg.EventoId, msg.Titulo);

        var proyeccion = new EventoProyeccion(msg.EventoId, msg.Titulo, msg.Categoria, msg.FechaInicio);
        await _repositorio.AgregarEventoAsync(proyeccion);
    }
}

public class EventoCanceladoConsumer : IConsumer<EventoCanceladoEvento>
{
    private readonly IRepositorioRecomendaciones _repositorio;
    private readonly ILogger<EventoCanceladoConsumer> _logger;

    public EventoCanceladoConsumer(IRepositorioRecomendaciones repositorio, ILogger<EventoCanceladoConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EventoCanceladoEvento> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Cancelando proyección de evento {EventoId}", msg.EventoId);

        var proyeccion = await _repositorio.ObtenerEventoAsync(msg.EventoId);
        if (proyeccion != null)
        {
            proyeccion.Desactivar();
            await _repositorio.ActualizarEventoAsync(proyeccion);
        }
    }
}
