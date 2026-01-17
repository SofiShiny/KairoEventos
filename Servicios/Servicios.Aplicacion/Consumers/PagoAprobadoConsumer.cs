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
        _logger.LogInformation("Procesando pago aprobado para Reserva/Orden: {OrdenId}", message.OrdenId);

        var reservasAConfirmar = new List<Dominio.Entidades.ReservaServicio>();

        // 1. Intentar buscar por ID directo (Caso: Pago individual de servicio)
        var reservaDirecta = await _repositorio.ObtenerReservaPorIdAsync(message.OrdenId);
        if (reservaDirecta != null) 
        {
            reservasAConfirmar.Add(reservaDirecta);
        }

        // 2. Intentar buscar vinculadas a la Orden de Entradas (Caso: Pago conjunto)
        var reservasVinculadas = await _repositorio.ObtenerReservasPorOrdenEntradaAsync(message.OrdenId);
        if (reservasVinculadas != null && reservasVinculadas.Any())
        {
            reservasAConfirmar.AddRange(reservasVinculadas);
        }

        // Eliminar duplicados si los hubiere
        reservasAConfirmar = reservasAConfirmar.DistinctBy(r => r.Id).ToList();

        if (!reservasAConfirmar.Any())
        {
            _logger.LogWarning("Se recibió pago {OrdenId} pero no se encontraron reservas de servicios asociadas.", message.OrdenId);
            return;
        }

        foreach (var reserva in reservasAConfirmar)
        {
            try 
            {
                if (reserva.Estado == Dominio.Entidades.EstadoReserva.PendientePago)
                {
                    reserva.ConfirmarPago();
                    await _repositorio.ActualizarReservaAsync(reserva);
                    _logger.LogInformation("Reserva {Id} confirmada exitosamente tras pago {OrdenId}", reserva.Id, message.OrdenId);
                }
                else
                {
                     _logger.LogInformation("Reserva {Id} ya estaba en estado {Estado}, se omite actualización", reserva.Id, reserva.Estado);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar la reserva {Id}", reserva.Id);
            }
        }
    }
}
