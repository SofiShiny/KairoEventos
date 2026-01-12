using MediatR;
using Entradas.Aplicacion.DTOs;

namespace Entradas.Aplicacion.Queries;

/// <summary>
/// Query para obtener el historial completo de entradas de un usuario
/// Incluye información enriquecida del evento y asiento
/// </summary>
/// <param name="UsuarioId">Identificador único del usuario</param>
public record ObtenerHistorialUsuarioQuery(Guid UsuarioId) : IRequest<List<EntradaResumenDto>>;
