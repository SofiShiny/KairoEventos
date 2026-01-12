using MediatR;
using Entradas.Aplicacion.DTOs;

namespace Entradas.Aplicacion.Queries;

/// <summary>
/// Query para obtener recomendaciones de eventos para un usuario
/// </summary>
/// <param name="UsuarioId">ID del usuario</param>
/// <param name="Cantidad">Cantidad de recomendaciones deseadas</param>
public record ObtenerRecomendacionesQuery(
    Guid UsuarioId, 
    int Cantidad = 3
) : IRequest<IEnumerable<EventoRecomendadoDto>>;
