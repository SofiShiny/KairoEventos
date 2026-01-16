using MassTransit;
using Microsoft.Extensions.Logging;
using Entradas.Dominio.Interfaces;
using Pagos.Aplicacion.Eventos;
using Entradas.Dominio.Enums;
using Entradas.Dominio.Eventos;

namespace Entradas.Aplicacion.Consumers;

/// <summary>
/// Procesa el rechazo de un pago para cancelar la entrada correspondiente
/// </summary>
public class PagoRechazadoConsumer : IConsumer<PagoRechazadoEvento>
{
    private readonly IRepositorioEntradas _repositorio;
    private readonly ILogger<PagoRechazadoConsumer> _logger;

    public PagoRechazadoConsumer(
        IRepositorioEntradas repositorio,
        ILogger<PagoRechazadoConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PagoRechazadoEvento> context)
    {
        var mensaje = context.Message;
        
        _logger.LogWarning("Recibido PagoRechazadoEvento para OrdenId: {OrdenId}. Motivo: {Motivo}", 
            mensaje.OrdenId, mensaje.Motivo);

        try
        {
            var entrada = await _repositorio.ObtenerPorIdAsync(mensaje.OrdenId, context.CancellationToken);

            if (entrada != null && (entrada.Estado == EstadoEntrada.PendientePago || entrada.Estado == EstadoEntrada.Reservada))
            {
                _logger.LogInformation("Cancelando entrada {EntradaId} por rechazo de pago", entrada.Id);
                entrada.Cancelar();
                await _repositorio.GuardarAsync(entrada, context.CancellationToken);

                // Notificar que la reserva se canceló para liberar el asiento inmediatamente
                await context.Publish(new ReservaCanceladaEvento(
                    entrada.Id,
                    entrada.AsientoId,
                    entrada.EventoId,
                    entrada.UsuarioId,
                    DateTime.UtcNow
                ));
                
                _logger.LogInformation("Entrada {EntradaId} cancelada y asiento liberado por rechazo de pago", entrada.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando PagoRechazadoEvento para OrdenId {OrdenId}", mensaje.OrdenId);
        }
    }
}
