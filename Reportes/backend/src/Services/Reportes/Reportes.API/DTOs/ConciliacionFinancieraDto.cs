namespace Reportes.API.DTOs;

public class ConciliacionFinancieraDto
{
    public decimal TotalIngresos { get; set; }
    public int CantidadTransacciones { get; set; }
    public Dictionary<string, decimal> DesglosePorCategoria { get; set; } = new();
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public List<TransaccionDto> Transacciones { get; set; } = new();
}

public class TransaccionDto
{
    public Guid EventoId { get; set; }
    public string TituloEvento { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int CantidadReservas { get; set; }
    public decimal Monto { get; set; }
}
