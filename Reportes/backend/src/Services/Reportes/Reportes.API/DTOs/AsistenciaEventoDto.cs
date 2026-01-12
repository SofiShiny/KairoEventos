namespace Reportes.API.DTOs;

public class AsistenciaEventoDto
{
    public Guid EventoId { get; set; }
    public string TituloEvento { get; set; } = string.Empty;
    public int TotalAsistentes { get; set; }
    public int AsientosReservados { get; set; }
    public int AsientosDisponibles { get; set; }
    public int CapacidadTotal { get; set; }
    public double PorcentajeOcupacion { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}
