namespace Reportes.API.DTOs;

public class LogAuditoriaDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string TipoOperacion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public string Detalles { get; set; } = string.Empty;
    public bool Exitoso { get; set; }
    public string? MensajeError { get; set; }
}
