using MassTransit;
using Microsoft.Extensions.Logging;
using Pagos.Aplicacion.Eventos; // Usamos el namespace del publisher de pagos
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;

namespace Reportes.Aplicacion.Consumers;

/// <summary>
/// Consumer que escucha PagoAprobadoEvento y actualiza las métricas diarias
/// </summary>
public class VentaRegistradaConsumer : IConsumer<PagoAprobadoEvento>
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<VentaRegistradaConsumer> _logger;

    public VentaRegistradaConsumer(
        IRepositorioReportesLectura repositorio, 
        ILogger<VentaRegistradaConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PagoAprobadoEvento> context)
    {
        var mensaje = context.Message;
        _logger.LogInformation("Venta aprobada recibida: {TransaccionId} por ${Monto}", 
            mensaje.TransaccionId, mensaje.Monto);

        try
        {
            var fecha = DateTime.UtcNow.Date;
            
            // Nota: En un sistema real, sacaríamos el EventoId de la Orden 
            // Para este ejemplo, usaremos un Guid vacío o uno asociado a la transacción si estuviera disponible
            // ya que PagoAprobadoEvento no trae el EventoId directamente.
            var eventoId = Guid.Empty; 

            var metricas = await _repositorio.ObtenerMetricasDiariasAsync(fecha, eventoId);

            if (metricas == null)
            {
                metricas = new MetricasDiarias
                {
                    Fecha = fecha,
                    EventoId = eventoId,
                    TotalVentas = 0,
                    EntradasVendidas = 0
                };
            }

            metricas.TotalVentas += mensaje.Monto;
            metricas.EntradasVendidas += 1; // Asumimos 1 entrada por pago aprobado en este flujo simple

            await _repositorio.ActualizarMetricasDiariasAsync(metricas);
            
            _logger.LogInformation("Métricas diarias actualizadas para {Fecha}. Total: ${Total}", 
                fecha.ToShortDateString(), metricas.TotalVentas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar métricas para la transacción {TransaccionId}", 
                mensaje.TransaccionId);
            throw;
        }
    }
}
