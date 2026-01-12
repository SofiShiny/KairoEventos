using MediatR;
using Usuarios.Application.Dtos;

namespace Usuarios.Application.Handlers.Commands.Command
{
    public class AgregarUsuarioCommand: IRequest<AgregarUsuarioDto>
    {
        public required AgregarUsuarioDto AgregarUsuariotDto { get; set; }
    }
}
