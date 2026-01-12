using MediatR;
using Entradas.Aplicacion.DTOs;

namespace Entradas.Aplicacion.Queries;

public record ObtenerTodasLasEntradasQuery(Guid? OrganizadorId = null) : IRequest<IEnumerable<EntradaDto>>;
