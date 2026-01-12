using Reportes.Aplicacion.DTOs;

namespace Reportes.API.DTOs;

public class MetricasEventoDto
{
    public Guid EventoId { get; set; }
    public string TituloEvento { get; set; } = string.Empty;
    public int TotalAsistentes { get; set; }
    public int TotalReservas { get; set; }
    public decimal IngresoTotal { get; set; }
    public decimal TotalDescuentos { get; set; }
    public List<CuponUsoDto> TopCupones { get; set; } = new();
    public string Estado { get; set; } = string.Empty;
    public DateTime? FechaPublicacion { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}
