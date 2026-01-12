using MediatR;
using Entradas.Aplicacion.DTOs;

namespace Entradas.Aplicacion.Queries;

/// <summary>
/// Query para obtener una entrada por su ID
/// </summary>
/// <param name="EntradaId">Identificador único de la entrada</param>
public record ObtenerEntradaQuery(Guid EntradaId) : IRequest<EntradaDto?>;