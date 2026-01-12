namespace Eventos.Aplicacion.DTOs;

using System;
using System.Collections.Generic;
using System.Linq;

// DTO específico para respuestas de API
// Propiedades requeridas no null para garantizar datos completos
// Usa AsistenteResponseDto para consistencia en la respuesta
public class EventoResponseDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public UbicacionDto? Ubicacion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int MaximoAsistentes { get; set; }
    public int ConteoAsistentesActual { get; set; }
    public string? Estado { get; set; }
    public string? OrganizadorId { get; set; }
    public DateTime CreadoEn { get; set; }
    public IEnumerable<AsistenteResponseDto> Asistentes { get; set; } = Enumerable.Empty<AsistenteResponseDto>();
}