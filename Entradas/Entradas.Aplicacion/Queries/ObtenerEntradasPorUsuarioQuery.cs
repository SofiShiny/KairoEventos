using MediatR;
using Entradas.Aplicacion.DTOs;

namespace Entradas.Aplicacion.Queries;

/// <summary>
/// Query para obtener todas las entradas de un usuario específico
/// </summary>
/// <param name="UsuarioId">Identificador único del usuario</param>
public record ObtenerEntradasPorUsuarioQuery(Guid UsuarioId) : IRequest<IEnumerable<EntradaDto>>;