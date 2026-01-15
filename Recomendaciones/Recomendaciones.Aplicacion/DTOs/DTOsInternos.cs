namespace Recomendaciones.Aplicacion.DTOs;

// DTOs internos para mapear respuestas de otros microservicios
internal class EventoInternoDto
{
    public Guid Id { get; set; }
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? UrlImagen { get; set; }
    public UbicacionInternaDto? Ubicacion { get; set; }
    public int MaximoAsistentes { get; set; }
    public int ConteoAsistentesActual { get; set; }
}

internal class UbicacionInternaDto
{
    public string? NombreLugar { get; set; }
    public string? Ciudad { get; set; }
}

internal class EntradaInternaDto
{
    public Guid Id { get; set; }
    public string? Categoria { get; set; }
    public string? TituloEvento { get; set; }
}
