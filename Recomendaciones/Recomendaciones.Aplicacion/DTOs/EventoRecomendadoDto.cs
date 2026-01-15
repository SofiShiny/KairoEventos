namespace Recomendaciones.Aplicacion.DTOs;

/// <summary>
/// DTO para eventos recomendados en el sistema
/// </summary>
public class EventoRecomendadoDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? UrlImagen { get; set; }
    public string? NombreLugar { get; set; }
    public string? Ciudad { get; set; }
    public int EntradasVendidas { get; set; }
    public decimal PrecioDesde { get; set; }
}

/// <summary>
/// Respuesta para recomendaciones personalizadas
/// </summary>
public class RecomendacionesPersonalizadasDto
{
    public string TipoRecomendacion { get; set; } = string.Empty; // "Personalizado" o "Tendencias"
    public string? CategoriaFavorita { get; set; }
    public string TituloSeccion { get; set; } = string.Empty; // "Porque te gusta Rock" o "Lo más vendido hoy"
    public List<EventoRecomendadoDto> Eventos { get; set; } = new();
}
