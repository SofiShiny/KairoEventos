namespace Reportes.API.DTOs;

public class ResumenVentasDto
{
    public decimal TotalVentas { get; set; }
    public int CantidadReservas { get; set; }
    public double PromedioEvento { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public List<VentaPorEventoDto> VentasPorEvento { get; set; } = new();
}

public class VentaPorEventoDto
{
    public Guid EventoId { get; set; }
    public string TituloEvento { get; set; } = string.Empty;
    public int CantidadReservas { get; set; }
    public decimal TotalIngresos { get; set; }
}
