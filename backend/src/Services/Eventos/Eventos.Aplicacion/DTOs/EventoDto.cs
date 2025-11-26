namespace Eventos.Aplicacion.DTOs;

// DTO usado internamente en la aplicación
// Propiedades nullable para flexibilidad en mapeo desde entidad de dominio
public class EventoDto
{
    public Guid Id { get; set; }
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public UbicacionDto? Ubicacion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int MaximoAsistentes { get; set; }
    public int ConteoAsistentesActual { get; set; }
    public string? Estado { get; set; }
    public string? OrganizadorId { get; set; }
    public DateTime CreadoEn { get; set; }
    public IEnumerable<AsistenteDto>? Asistentes { get; set; }
}
