using MediatR;
using Usuarios.Aplicacion.DTOs;

namespace Usuarios.Aplicacion.Consultas;

public record ConsultarUsuarioQuery : IRequest<UsuarioDto?>
{
    public Guid UsuarioId { get; init; }
}
