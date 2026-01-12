using System.ComponentModel.DataAnnotations;

namespace Eventos.Aplicacion.DTOs;

// Permite controlar que datos acepta la API y validar en la capa de presentación
public class EventoCreateDto
{
    // [Required] habilita validación automática en ASP.NET Core antes de llegar al handler
    [Required] public string Titulo { get; set; } = string.Empty;
    
    [Required] public string? Descripcion { get; set; }
    
    // La ubicación es un objeto anidado porque es un Value Object
    [Required] public UbicacionDto? Ubicacion { get; set; }
    
    [Required] public DateTime FechaInicio { get; set; }
    
    [Required] public DateTime FechaFin { get; set; }
    
    // Range asegura que siempre haya al menos 1 asistente permitido
    [Range(1, int.MaxValue)] public int MaximoAsistentes { get; set; } = 1;
    
    // Campos opcionales
    public string? Estado { get; set; }
    public string? Categoria { get; set; }
    public List<AsistenteCreateDto>? Asistentes { get; set; }
}