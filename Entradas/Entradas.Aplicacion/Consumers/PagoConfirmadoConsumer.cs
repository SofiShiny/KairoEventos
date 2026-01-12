using MassTransit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Entradas.Dominio.Interfaces;
using Entradas.Dominio.Eventos;
using Entradas.Dominio.Excepciones;

namespace Entradas.Aplicacion.Consumers;

/// <summary>
/// Consumer que procesa eventos de pago confirmado para actualizar el estado de las entradas
/// </summary>
public class PagoConfirmadoConsumer : IConsumer<PagoConfirmadoEvento>
{
    private readonly IRepositorioEntradas _repositorio;
    private readonly ILogger<PagoConfirmadoConsumer> _logger;
    private readonly IEntradasMetrics _metrics;
    private readonly ActivitySource _activitySource;

    public PagoConfirmadoConsumer(
        IRepositorioEntradas repositorio,
        ILogger<PagoConfirmadoConsumer> logger,
        IEntradasMetrics metrics,
        ActivitySource activitySource)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
    }

    public async Task Consume(ConsumeContext<PagoConfirmadoEvento> context)
    {
        var mensaje = context.Message;
        var stopwatch = Stopwatch.StartNew();
        
        using var activity = _activitySource.StartActivity("ProcesarPagoConfirmado");
        activity?.SetTag("entrada.id", mensaje.EntradaId.ToString());
        activity?.SetTag("transaccion.id", mensaje.TransaccionId.ToString());
        activity?.SetTag("monto.confirmado", mensaje.MontoConfirmado.ToString());
        
        _logger.LogInformation("Procesando pago confirmado para entrada {EntradaId}, transacción {TransaccionId}", 
            mensaje.EntradaId, mensaje.TransaccionId);

        try
        {
            // 1. Localizar la entrada correspondiente
            var entrada = await _repositorio.ObtenerPorIdAsync(mensaje.EntradaId, context.CancellationToken);
            
            if (entrada == null)
            {
                stopwatch.Stop();
                _logger.LogWarning("Entrada {EntradaId} no encontrada para pago confirmado. Transacción: {TransaccionId}", 
                    mensaje.EntradaId, mensaje.TransaccionId);
                
                _metrics.IncrementPagosConfirmados("entrada_no_encontrada");
                activity?.SetStatus(ActivityStatusCode.Error, "Entrada no encontrada");
                
                // No lanzamos excepción para evitar reintento infinito
                // El mensaje se considera procesado pero se registra el problema
                return;
            }

            _logger.LogDebug("Entrada {EntradaId} encontrada en estado {Estado}", 
                entrada.Id, entrada.Estado);

            // 2. Validar que el monto coincide (opcional pero recomendado)
            if (entrada.Monto != mensaje.MontoConfirmado)
            {
                _logger.LogWarning("Monto de entrada {EntradaId} ({MontoEntrada}) no coincide con monto confirmado ({MontoConfirmado})", 
                    entrada.Id, entrada.Monto, mensaje.MontoConfirmado);
                
                activity?.SetTag("monto.discrepancia", "true");
                // Continuamos el procesamiento pero registramos la discrepancia
            }

            // 3. Cambiar estado de PendientePago a Pagada
            entrada.ConfirmarPago();
            
            _logger.LogDebug("Estado de entrada {EntradaId} cambiado a {NuevoEstado}", 
                entrada.Id, entrada.Estado);

            // 4. Persistir el cambio en base de datos
            await _repositorio.GuardarAsync(entrada, context.CancellationToken);
            
            stopwatch.Stop();
            _logger.LogInformation("Pago confirmado procesado exitosamente para entrada {EntradaId}. Estado actualizado a {Estado}", 
                entrada.Id, entrada.Estado);

            // 5. Registrar métricas de éxito
            _metrics.IncrementPagosConfirmados("success");
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("resultado", "success");
            activity?.SetTag("estado.final", entrada.Estado.ToString());
        }
        catch (EntradaNoEncontradaException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "Entrada {EntradaId} no encontrada al procesar pago confirmado", mensaje.EntradaId);
            _metrics.IncrementPagosConfirmados("entrada_no_encontrada");
            activity?.SetStatus(ActivityStatusCode.Error, "Entrada no encontrada");
            // No relanzamos la excepción para evitar reintento infinito
        }
        catch (EstadoEntradaInvalidoException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "Estado inválido para entrada {EntradaId} al confirmar pago", mensaje.EntradaId);
            _metrics.IncrementPagosConfirmados("estado_invalido");
            activity?.SetStatus(ActivityStatusCode.Error, "Estado inválido");
            // No relanzamos la excepción para evitar reintento infinito
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error inesperado al procesar pago confirmado para entrada {EntradaId}", mensaje.EntradaId);
            _metrics.IncrementPagosConfirmados("error");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            // Relanzamos para que MassTransit maneje el reintento
            throw;
        }
    }
}