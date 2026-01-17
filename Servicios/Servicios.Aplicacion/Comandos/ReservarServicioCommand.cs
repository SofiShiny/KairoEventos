using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;
using Servicios.Dominio.Entidades;
using Servicios.Dominio.Repositorios;
using Servicios.Aplicacion.Eventos;

namespace Servicios.Aplicacion.Comandos;

public record ReservarServicioCommand(
    Guid UsuarioId, 
    Guid EventoId, 
    Guid ServicioGlobalId,
    Guid? OrdenEntradaId = null
) : IRequest<Guid>;

public class ReservarServicioCommandHandler : IRequestHandler<ReservarServicioCommand, Guid>
{
    private readonly IRepositorioServicios _repositorio;
    private readonly IVerificadorEntradas _verificadorEntradas;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ReservarServicioCommandHandler> _logger;

    public ReservarServicioCommandHandler(
        IRepositorioServicios repositorio,
        IVerificadorEntradas verificadorEntradas,
        IPublishEndpoint publishEndpoint,
        ILogger<ReservarServicioCommandHandler> logger)
    {
        _repositorio = repositorio;
        _verificadorEntradas = verificadorEntradas;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Guid> Handle(ReservarServicioCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar que el servicio exista y esté activo
        var servicio = await _repositorio.ObtenerServicioPorIdAsync(request.ServicioGlobalId);
        if (servicio == null || !servicio.Activo)
        {
            throw new InvalidOperationException("El servicio solicitado no está disponible");
        }

        // 2. Validar que el usuario tenga entrada para el evento (Consulta Síncrona a Entradas.API)
        _logger.LogInformation("Verificando entrada para usuario {UsuarioId} en evento {EventoId}", request.UsuarioId, request.EventoId);
        var tieneEntrada = await _verificadorEntradas.UsuarioTieneEntradaParaEventoAsync(request.UsuarioId, request.EventoId);
        
        if (!tieneEntrada)
        {
            _logger.LogWarning("Reserva rechazada: El usuario {UsuarioId} no tiene una entrada válida para el evento {EventoId}", request.UsuarioId, request.EventoId);
            throw new UnauthorizedAccessException("Debe tener una entrada válida para el evento para poder contratar servicios extras");
        }

        // 3. Crear la reserva en estado PendientePago
        var reserva = new ReservaServicio(request.UsuarioId, request.EventoId, request.ServicioGlobalId, request.OrdenEntradaId);
        await _repositorio.AgregarReservaAsync(reserva);

        _logger.LogInformation("Reserva de servicio {ReservaId} creada en estado PendientePago (OrdenVinculada: {OrdenId})", reserva.Id, request.OrdenEntradaId);

        // 4. Si NO está vinculada a una orden de entrada, iniciamos flujo de pago individual
        if (request.OrdenEntradaId == null)
        {
            await _publishEndpoint.Publish(new SolicitudPagoServicioCreada(
                reserva.Id,
                request.UsuarioId,
                servicio.Precio,
                $"Servicio Extra: {servicio.Nombre}"
            ), cancellationToken);
        }

        return reserva.Id;
    }
}
