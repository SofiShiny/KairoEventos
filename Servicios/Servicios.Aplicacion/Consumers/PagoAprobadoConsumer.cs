using MassTransit;
using Microsoft.Extensions.Logging;
using Servicios.Dominio.Repositorios;
using Servicios.Aplicacion.Eventos;

namespace Servicios.Aplicacion.Consumers;

public class PagoAprobadoConsumer : IConsumer<PagoAprobadoEvento>
{
    private readonly IRepositorioServicios _repositorio;
    private readonly ILogger<PagoAprobadoConsumer> _logger;

    public PagoAprobadoConsumer(IRepositorioServicios repositorio, ILogger<PagoAprobadoConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PagoAprobadoEvento> context)
    {
        var message = context.Message;
        _logger.LogInformation("Procesando pago aprobado para Reserva: {ReservaId}", message.OrdenId);

        // Intentar obtener la reserva. Usamos OrdenId del mensaje como nuestro ReservaId
        var reserva = await _repositorio.ObtenerReservaPorIdAsync(message.OrdenId);
        
        if (reserva == null)
        {
            _logger.LogWarning("Se recibió pago para una reserva inexistente o de otro módulo: {Id}", message.OrdenId);
            return;
        }

        try 
        {
            reserva.ConfirmarPago();
            await _repositorio.ActualizarReservaAsync(reserva);
            _logger.LogInformation("Reserva {Id} confirmada exitosamente tras pago", reserva.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar la reserva {Id}", reserva.Id);
        }
    }
}
