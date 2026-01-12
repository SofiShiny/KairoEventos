using MediatR;
using Usuarios.Aplicacion.DTOs;

namespace Usuarios.Aplicacion.Consultas;

public record ConsultarUsuariosQuery : IRequest<IEnumerable<UsuarioDto>>
{
}
