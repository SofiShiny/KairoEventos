using Usuarios.Application.Dtos;
using MediatR;

namespace Usuarios.Application.Handlers.Commands.Command
{
    public class ActualizarUsuarioCommand : IRequest<ActualizarUsuarioDto>
    {
        public required ActualizarUsuarioDto ActualizarUsuarioDto { get; set; }
        public required Guid Id { get; set; }
    }
}
