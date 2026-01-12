using MassTransit;
using Microsoft.Extensions.Logging;
using Entradas.Dominio.Interfaces;
using Pagos.Aplicacion.Eventos;
using Asientos.Dominio.EventosDominio;
using Entradas.Dominio.Enums;

namespace Entradas.Aplicacion.Consumers;

/// <summary>
/// Procesa la liberación de un asiento para invalidar la entrada correspondiente si está en estado pendiente.
/// </summary>
public class AsientoLiberadoConsumer : IConsumer<AsientoLiberadoEventoDominio>
{
    private readonly IRepositorioEntradas _repositorio;
    private readonly ILogger<AsientoLiberadoConsumer> _logger;

    public AsientoLiberadoConsumer(
        IRepositorioEntradas repositorio,
        ILogger<AsientoLiberadoConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AsientoLiberadoEventoDominio> context)
    {
        var mensaje = context.Message;
        
        _logger.LogInformation("Recibido AsientoLiberadoEventoDominio. AsientoId: {AsientoId}, Fila: {Fila}, Numero: {Numero}", 
            mensaje.AsientoId, mensaje.Fila, mensaje.Numero);

        try
        {
            // Buscar la entrada activa para este asiento
            var entrada = await _repositorio.ObtenerActivaPorAsientoAsync(mensaje.AsientoId, context.CancellationToken);

            if (entrada != null)
            {
                if (entrada.Estado == EstadoEntrada.PendientePago || entrada.Estado == EstadoEntrada.Reservada)
                {
                    _logger.LogInformation("Cancelando entrada {EntradaId} porque el asiento {AsientoId} fue liberado.", entrada.Id, mensaje.AsientoId);
                    entrada.Cancelar();
                    await _repositorio.GuardarAsync(entrada, context.CancellationToken);
                }
                else
                {
                    _logger.LogDebug("Se recibió liberación para asiento {AsientoId}, pero la entrada {EntradaId} ya está en estado {Estado}. No se cancelará automáticamente.", 
                        mensaje.AsientoId, entrada.Id, entrada.Estado);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando AsientoLiberadoEvento para AsientoId {AsientoId}", mensaje.AsientoId);
            // No hacemos throw para evitar reintentos infinitos si es un error de lógica, 
            // pero podrías habilitarlo si quieres que el mensaje se re-procese.
        }
    }
}
