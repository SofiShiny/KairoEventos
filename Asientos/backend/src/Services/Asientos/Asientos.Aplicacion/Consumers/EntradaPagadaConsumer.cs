using MassTransit;
using Microsoft.Extensions.Logging;
using Asientos.Dominio.Repositorios;
using Entradas.Dominio.Eventos;

namespace Asientos.Aplicacion.Consumers;

public class EntradaPagadaConsumer : IConsumer<EntradaPagadaEvento>
{
    private readonly IRepositorioMapaAsientos _repositorio;
    private readonly ILogger<EntradaPagadaConsumer> _logger;

    public EntradaPagadaConsumer(
        IRepositorioMapaAsientos repositorio,
        ILogger<EntradaPagadaConsumer> logger)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<EntradaPagadaEvento> context)
    {
        var mensaje = context.Message;
        _logger.LogInformation("🎫 Recibido EntradaPagadaEvento para Orden {OrdenId}. Asientos a confirmar: {Count}", 
            mensaje.OrdenId, mensaje.AsientosIds.Count);

        if (!mensaje.AsientosIds.Any())
        {
            _logger.LogWarning("⚠️ Evento recibido sin AsientosIds. Orden {OrdenId}", mensaje.OrdenId);
            return;
        }

        try
        {
            var procesados = 0;
            foreach (var asientoId in mensaje.AsientosIds)
            {
                var asiento = await _repositorio.ObtenerAsientoPorIdAsync(asientoId, context.CancellationToken);
                if (asiento != null)
                {
                    if (asiento.Reservado)
                    {
                        asiento.MarcarComoPagado();
                        procesados++;
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Intento de pagar asiento {AsientoId} que NO está reservado.", asientoId);
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ Asiento {AsientoId} no encontrado.", asientoId);
                }
            }

            if (procesados > 0)
            {
                await _repositorio.GuardarCambiosAsync(context.CancellationToken);
                _logger.LogInformation("✅ {Count} asientos marcados como PAGADOS y OCUPADOS.", procesados);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al procesar EntradaPagadaEvento. Orden {OrdenId}", mensaje.OrdenId);
            throw;
        }
    }
}
