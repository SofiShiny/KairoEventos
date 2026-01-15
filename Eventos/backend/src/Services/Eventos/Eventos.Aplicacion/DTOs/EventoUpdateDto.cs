namespace Eventos.Aplicacion.DTOs;

using System;
using System.Collections.Generic;

public class EventoUpdateDto
{
    // Todas opcionales para permitir updates parciales (PATCH)
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public UbicacionDto? Ubicacion { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? MaximoAsistentes { get; set; }
    public string? Estado { get; set; }
    public decimal? PrecioBase { get; set; }
    public System.Collections.Generic.List<AsistenteUpdateDto>? Asistentes { get; set; }
}