using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notificaciones.Aplicacion.Interfaces;

namespace Notificaciones.Aplicacion.Consumers;

/// <summary>
/// Consumidor que procesa eventos de cancelación de reservas/entradas
/// </summary>
public class ReservaCanceladaConsumer : IConsumer<ReservaCanceladaEvento>
{
    private readonly INotificador _notificador;
    private readonly ILogger<ReservaCanceladaConsumer> _logger;

    public ReservaCanceladaConsumer(
        INotificador notificador,
        ILogger<ReservaCanceladaConsumer> logger)
    {
        _notificador = notificador ?? throw new ArgumentNullException(nameof(notificador));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<ReservaCanceladaEvento> context)
    {
        var mensaje = context.Message;
        
        _logger.LogInformation(
            "Procesando cancelación de entrada {EntradaId} para usuario {UsuarioId}",
            mensaje.EntradaId,
            mensaje.UsuarioId);

        try
        {
            // Enviar notificación de reembolso al usuario
            await _notificador.EnviarNotificacionUsuario(
                mensaje.UsuarioId.ToString(),
                new
                {
                    tipo = "entrada_cancelada",
                    titulo = "💰 Reembolso Procesado",
                    mensaje = "Tu entrada ha sido cancelada exitosamente. El reembolso se procesará en 3-5 días hábiles.",
                    datos = new
                    {
                        entradaId = mensaje.EntradaId,
                        eventoId = mensaje.EventoId,
                        asientoId = mensaje.AsientoId,
                        fechaCancelacion = mensaje.FechaCancelacion
                    }
                });

            _logger.LogInformation(
                "Notificación de cancelación enviada exitosamente para entrada {EntradaId}",
                mensaje.EntradaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al procesar notificación de cancelación para entrada {EntradaId}",
                mensaje.EntradaId);
            throw;
        }
    }
}

/// <summary>
/// Evento de integración para cancelación de reservas
/// </summary>
public record ReservaCanceladaEvento(
    Guid EntradaId,
    Guid? AsientoId,
    Guid EventoId,
    Guid UsuarioId,
    DateTime FechaCancelacion
);
