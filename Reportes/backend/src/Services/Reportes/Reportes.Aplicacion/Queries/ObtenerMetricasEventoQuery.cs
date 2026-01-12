using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Reportes.Aplicacion.DTOs;
using Reportes.Dominio.Repositorios;

namespace Reportes.Aplicacion.Queries;

/// <summary>
/// Query para obtener las métricas de un evento específico
/// </summary>
public class ObtenerMetricasEventoQuery
{
    public Guid EventoId { get; set; }
}

/// <summary>
/// Handler para obtener métricas de evento con información de descuentos y cupones
/// </summary>
public class ObtenerMetricasEventoQueryHandler
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<ObtenerMetricasEventoQueryHandler> _logger;

    public ObtenerMetricasEventoQueryHandler(
        IRepositorioReportesLectura repositorio,
        ILogger<ObtenerMetricasEventoQueryHandler> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task<MetricasEventoDto?> Handle(ObtenerMetricasEventoQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo métricas para evento {EventoId}", query.EventoId);

        var metricas = await _repositorio.ObtenerMetricasEventoAsync(query.EventoId);

        if (metricas == null)
        {
            _logger.LogWarning("No se encontraron métricas para el evento {EventoId}", query.EventoId);
            return null;
        }

        // Mapear a DTO y ordenar cupones por uso (Top 5)
        var dto = new MetricasEventoDto
        {
            EventoId = metricas.EventoId,
            TituloEvento = metricas.TituloEvento,
            FechaInicio = metricas.FechaInicio,
            Estado = metricas.Estado,
            TotalAsistentes = metricas.TotalAsistentes,
            TotalReservas = metricas.TotalReservas,
            IngresoTotal = metricas.IngresoTotal,
            TotalDescuentos = metricas.TotalDescuentos,
            TopCupones = metricas.UsoDeCupones
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp => new CuponUsoDto
                {
                    Codigo = kvp.Key,
                    Cantidad = kvp.Value
                })
                .ToList(),
            FechaCreacion = metricas.FechaCreacion,
            UltimaActualizacion = metricas.UltimaActualizacion
        };

        _logger.LogInformation("Métricas obtenidas exitosamente. Top cupones: {Count}", dto.TopCupones.Count);

        return dto;
    }
}
