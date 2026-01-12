namespace Entradas.Aplicacion.DTOs;

public record EventoRecomendadoDto(
    Guid Id,
    string Titulo,
    string? Categoria,
    DateTime FechaInicio,
    string? UrlImagen
);
