using Comunidad.Application.DTOs;
using MediatR;

namespace Comunidad.Application.Consultas;

public record ObtenerComentariosQuery(Guid EventoId) : IRequest<List<ComentarioDto>>;
