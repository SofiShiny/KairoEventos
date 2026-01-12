using MassTransit;
using Microsoft.Extensions.Logging;
using Entradas.Dominio.Interfaces;
using Pagos.Aplicacion.Eventos;
using Entradas.Dominio.Excepciones;
using Entradas.Dominio.Enums;

namespace Entradas.Aplicacion.Consumers;

/// <summary>
/// Procesa la confirmación de pago desde el microservicio de Pagos
/// Actualizado para manejar compras múltiples de manera robusta
/// </summary>
public class PagoAprobadoConsumer : IConsumer<PagoAprobadoEvento>
{
    private readonly IRepositorioEntradas _repositorio;
    private readonly IGeneradorCodigoQr _generadorQr;
    private readonly ILogger<PagoAprobadoConsumer> _logger;

    public PagoAprobadoConsumer(
        IRepositorioEntradas repositorio,
        IGeneradorCodigoQr generadorQr,
        ILogger<PagoAprobadoConsumer> logger)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _generadorQr = generadorQr ?? throw new ArgumentNullException(nameof(generadorQr));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<PagoAprobadoEvento> context)
    {
        var mensaje = context.Message;
        
        _logger.LogInformation(
            "🎫 Recibido PagoAprobadoEvento - OrdenId: {OrdenId}, TransaccionId: {TxId}, Monto: {Monto}", 
            mensaje.OrdenId, mensaje.TransaccionId, mensaje.Monto);

        try
        {
            // 1. Obtener TODAS las entradas asociadas a este OrdenId
            var entradas = await _repositorio.ObtenerPorOrdenIdAsync(mensaje.OrdenId, context.CancellationToken);

            if (entradas == null || !entradas.Any())
            {
                _logger.LogCritical(
                    "❌ ERROR CRÍTICO: No se encontraron entradas para OrdenId: {OrdenId}. TransaccionId: {TxId}", 
                    mensaje.OrdenId, mensaje.TransaccionId);
                
                // No lanzar excepción para evitar reintentos infinitos
                // Este es un caso de inconsistencia de datos que debe investigarse manualmente
                return;
            }

            _logger.LogInformation(
                "📋 Se encontraron {Cantidad} entrada(s) para confirmar. OrdenId: {OrdenId}", 
                entradas.Count, mensaje.OrdenId);

            // 2. Procesar cada entrada con manejo de idempotencia
            var entradasActualizadas = new List<Entradas.Dominio.Entidades.Entrada>();
            var yaConfirmadas = 0;
            var nuevasConfirmaciones = 0;

            foreach (var entrada in entradas)
            {
                try
                {
                    // IDEMPOTENCIA: Verificar si ya está confirmada
                    if (entrada.Estado == EstadoEntrada.Pagada)
                    {
                        _logger.LogInformation(
                            "✓ Entrada {EntradaId} ya estaba confirmada (idempotencia). Estado: {Estado}", 
                            entrada.Id, entrada.Estado);
                        yaConfirmadas++;
                        continue;
                    }

                    // Validar que esté en estado válido para confirmar
                    if (entrada.Estado != EstadoEntrada.Reservada && entrada.Estado != EstadoEntrada.PendientePago)
                    {
                        _logger.LogWarning(
                            "⚠️ Entrada {EntradaId} en estado inesperado: {Estado}. Se intentará confirmar de todos modos.", 
                            entrada.Id, entrada.Estado);
                    }

                    // 3. Confirmar el pago (cambia estado a Pagada)
                    entrada.ConfirmarPago();
                    _logger.LogDebug("✓ Estado actualizado a Pagada para entrada {EntradaId}", entrada.Id);

                    // 4. Generar QR final si es necesario
                    if (string.IsNullOrEmpty(entrada.CodigoQr) || entrada.CodigoQr.Contains("TEMP"))
                    {
                        var qrFinal = _generadorQr.GenerarCodigoUnico();
                        entrada.AsignarCodigoQr(qrFinal);
                        _logger.LogDebug("🎫 QR generado para entrada {EntradaId}: {Qr}", entrada.Id, qrFinal);
                    }

                    entradasActualizadas.Add(entrada);
                    nuevasConfirmaciones++;
                }
                catch (EstadoEntradaInvalidoException ex)
                {
                    _logger.LogWarning(
                        "⚠️ No se pudo confirmar entrada {EntradaId}: {Mensaje}. Se omitirá.", 
                        entrada.Id, ex.Message);
                    // Continuar con las demás entradas
                }
            }

            // 5. Persistir cambios en lote (más eficiente)
            if (entradasActualizadas.Any())
            {
                await _repositorio.ActualizarRangoAsync(entradasActualizadas, context.CancellationToken);
                
                _logger.LogInformation(
                    "✅ Pago confirmado exitosamente. OrdenId: {OrdenId}, TransaccionId: {TxId}, " +
                    "Nuevas confirmaciones: {Nuevas}, Ya confirmadas: {YaConfirmadas}, Total: {Total}",
                    mensaje.OrdenId, mensaje.TransaccionId, nuevasConfirmaciones, yaConfirmadas, entradas.Count);
            }
            else if (yaConfirmadas > 0)
            {
                _logger.LogInformation(
                    "ℹ️ Todas las entradas ya estaban confirmadas (idempotencia). OrdenId: {OrdenId}, Cantidad: {Cantidad}",
                    mensaje.OrdenId, yaConfirmadas);
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ No se pudo confirmar ninguna entrada. OrdenId: {OrdenId}, Total encontradas: {Total}",
                    mensaje.OrdenId, entradas.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "❌ Error inesperado procesando PagoAprobadoEvento. OrdenId: {OrdenId}, TransaccionId: {TxId}", 
                mensaje.OrdenId, mensaje.TransaccionId);
            
            // Relanzar para que MassTransit reintente
            throw;
        }
    }
}
