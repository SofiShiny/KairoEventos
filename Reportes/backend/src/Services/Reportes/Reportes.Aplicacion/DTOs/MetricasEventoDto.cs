using System;
using System.Collections.Generic;

namespace Reportes.Aplicacion.DTOs;

/// <summary>
/// DTO que representa las métricas completas de un evento
/// </summary>
public class MetricasEventoDto
{
    public Guid EventoId { get; set; }
    public string TituloEvento { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int TotalAsistentes { get; set; }
    public int TotalReservas { get; set; }
    public decimal IngresoTotal { get; set; }
    public decimal TotalDescuentos { get; set; }
    public List<CuponUsoDto> TopCupones { get; set; } = new();
    public DateTime FechaCreacion { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}
