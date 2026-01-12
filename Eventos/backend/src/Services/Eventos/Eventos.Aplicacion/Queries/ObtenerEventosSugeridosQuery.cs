using BloquesConstruccion.Aplicacion.Queries;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Aplicacion.DTOs;

namespace Eventos.Aplicacion.Queries;

/// <summary>
/// Query para obtener eventos sugeridos basados en categoría y fecha
/// </summary>
/// <param name="Categoria">Categoría de interés</param>
/// <param name="FechaDesde">Fecha mínima de inicio (para evitar recomendar eventos pasados)</param>
/// <param name="Top">Cantidad máxima de eventos a retornar</param>
public record ObtenerEventosSugeridosQuery(
    string? Categoria = null, 
    DateTime? FechaDesde = null, 
    int Top = 3
) : IQuery<Resultado<IEnumerable<EventoDto>>>;
