using System;
using System.Threading.Tasks;
using Entradas.Dominio.Eventos;
using Eventos.Aplicacion.Comandos;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventos.Aplicacion.Consumers;

/// <summary>
/// Consumidor que escucha cuando se crea una entrada y registra al usuario como asistente del evento.
/// </summary>
public class EntradaCreadaConsumer : IConsumer<EntradaCreadaEvento>
{
    private readonly IMediator _mediator;
    private readonly ILogger<EntradaCreadaConsumer> _logger;

    public EntradaCreadaConsumer(IMediator mediator, ILogger<EntradaCreadaConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EntradaCreadaEvento> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consumiendo EntradaCreadaEvento para Evento: {EventoId}, Usuario: {UsuarioId}", 
            message.EventoId, message.UsuarioId);

        try
        {
            // Ejecutar el comando para registrar al asistente
            var command = new RegistrarAsistenteComando(
                message.EventoId,
                message.UsuarioId.ToString(),
                message.NombreUsuario ?? $"Usuario-{message.UsuarioId.ToString().Substring(0, 8)}",
                message.Email ?? "usuario@ejemplo.com"
            );

            var result = await _mediator.Send(command);

            if (result.EsExitoso)
            {
                _logger.LogInformation("Asistente {UsuarioId} registrado exitosamente en el evento {EventoId}", 
                    message.UsuarioId, message.EventoId);
            }
            else
            {
                _logger.LogWarning("No se pudo registrar al asistente {UsuarioId}: {Error}", 
                    message.UsuarioId, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al procesar registro de asistente para entrada {EntradaId}", message.EntradaId);
            // MassTransit reintentará según la política configurada
            throw;
        }
    }
}
