using MassTransit;
using Microsoft.Extensions.Logging;
using Entradas.Dominio.Eventos;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;
using System;
using System.Threading.Tasks;

namespace Reportes.Aplicacion.Consumers;

public class EntradaPagadaConsumer : IConsumer<EntradaPagadaEvento>
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<EntradaPagadaConsumer> _logger;

    public EntradaPagadaConsumer(
        IRepositorioReportesLectura repositorio, 
        ILogger<EntradaPagadaConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EntradaPagadaEvento> context)
    {
        var mensaje = context.Message;
        _logger.LogInformation("Reportes: Procesando EntradaPagadaEvento. Orden: {OrdenId}, Evento: {EventoId}, Monto: {Monto}", 
            mensaje.OrdenId, mensaje.EventoId, mensaje.MontoTotal);

        try
        {
            var fecha = DateTime.UtcNow.Date;
            var eventoId = mensaje.EventoId;

            // 1. Actualizar Métricas Diarias (desglose por evento)
            var metricas = await _repositorio.ObtenerMetricasDiariasAsync(fecha, eventoId);

            if (metricas == null)
            {
                metricas = new MetricasDiarias
                {
                    Fecha = fecha,
                    EventoId = eventoId, // Usamos el ID real del evento
                    TotalVentas = 0,
                    EntradasVendidas = 0
                };
            }

            metricas.TotalVentas += mensaje.MontoTotal;
            metricas.EntradasVendidas += mensaje.AsientosIds.Count;

            await _repositorio.ActualizarMetricasDiariasAsync(metricas);

            // 2. Actualizar Métricas Acumuladas del Evento
            var metricasEvento = await _repositorio.ObtenerMetricasEventoAsync(eventoId);
            if (metricasEvento != null)
            {
                // Convertir decimal a Decimal128 compatible si fuera necesario, pero C# maneja la conversión implicita suele funcionar con Driver.
                // Aseguramos que sumamos al ingreso total.
                metricasEvento.IngresoTotal += mensaje.MontoTotal;
                metricasEvento.UltimaActualizacion = DateTime.UtcNow;
                
                await _repositorio.ActualizarMetricasAsync(metricasEvento);
            }
            else
            {
                 _logger.LogWarning("Reportes: No se encontró MetricasEvento para {EventoId}. El evento podría no estar sincronizado.", eventoId);
            }

            _logger.LogInformation("Reportes: Métricas de venta actualizadas correctamente para Evento {EventoId}", eventoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando reportes para Orden {OrdenId}", mensaje.OrdenId);
            throw;
        }
    }
}
