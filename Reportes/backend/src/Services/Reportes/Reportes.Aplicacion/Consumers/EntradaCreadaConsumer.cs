using System;
using System.Threading.Tasks;
using Entradas.Dominio.Eventos;
using MassTransit;
using Microsoft.Extensions.Logging;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;

namespace Reportes.Aplicacion.Consumers;

public class EntradaCreadaConsumer : IConsumer<EntradaCreadaEvento>
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<EntradaCreadaConsumer> _logger;

    public EntradaCreadaConsumer(
        IRepositorioReportesLectura repositorio,
        ILogger<EntradaCreadaConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EntradaCreadaEvento> context)
    {
        var evento = context.Message;
        _logger.LogInformation("Procesando EntradaCreadaEvento para Evento: {EventoId}, Monto: {Monto}", 
            evento.EventoId, evento.Monto);

        try
        {
            var fecha = DateTime.UtcNow.Date;

            // 1. Actualizar Reporte de Ventas Diarias (General para el día)
            var reporteVentas = await _repositorio.ObtenerVentasDiariasAsync(fecha)
                ?? new ReporteVentasDiarias
                {
                    Fecha = fecha,
                    TituloEvento = "Consolidado Diario", // Título genérico si es nuevo
                    ReservasPorCategoria = new Dictionary<string, int>()
                };

            reporteVentas.TotalIngresos += evento.Monto;
            reporteVentas.CantidadReservas++;
            reporteVentas.UltimaActualizacion = DateTime.UtcNow;

            await _repositorio.ActualizarVentasDiariasAsync(reporteVentas);

            // 2. Actualizar Métricas del Evento (Específico por Evento)
            var metricas = await _repositorio.ObtenerMetricasEventoAsync(evento.EventoId);
            if (metricas != null)
            {
                metricas.IngresoTotal += evento.Monto;
                metricas.TotalReservas++; // Usado como CantidadEntradas
                metricas.TotalDescuentos += evento.MontoDescuento;
                metricas.UltimaActualizacion = DateTime.UtcNow;

                // Procesar Cupones
                if (!string.IsNullOrWhiteSpace(evento.CuponesAplicados))
                {
                    try
                    {
                        var cupones = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(evento.CuponesAplicados);
                        if (cupones != null)
                        {
                            foreach (var cupon in cupones)
                            {
                                if (metricas.UsoDeCupones.ContainsKey(cupon))
                                    metricas.UsoDeCupones[cupon]++;
                                else
                                    metricas.UsoDeCupones[cupon] = 1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al deserializar cupones para entrada {EntradaId}", evento.EntradaId);
                    }
                }

                await _repositorio.ActualizarMetricasAsync(metricas);
                
                _logger.LogInformation("Métricas actualizadas para evento {EventoId}. Descuento Total: {Descuento}, Cupones: {Count}", 
                    evento.EventoId, metricas.TotalDescuentos, metricas.UsoDeCupones.Count);
            }
            else
            {
                _logger.LogWarning("No se encontraron métricas base para el evento {EventoId}. Asegúrese de que el evento fue publicado correctamente.", 
                    evento.EventoId);
            }

            // 3. Registrar en Auditoría
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "VentaRegistrada",
                Entidad = "Entrada",
                EntidadId = evento.EntradaId.ToString(),
                Detalles = $"Venta registrada por monto {evento.Monto} para evento {evento.EventoId}",
                Exitoso = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar EntradaCreadaEvento para Evento {EventoId}", evento.EventoId);
            
            await _repositorio.RegistrarLogAuditoriaAsync(new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = "ErrorVentaRegistrada",
                Entidad = "Entrada",
                EntidadId = evento.EntradaId.ToString(),
                Detalles = "Error procesando registro de venta",
                Exitoso = false,
                MensajeError = ex.Message
            });

            throw; // Reintento
        }
    }
}
