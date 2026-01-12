using MediatR;
using Usuarios.Application.Dtos;

namespace Usuarios.Application.Handlers.Commands.Command;

public class RegistroTokenCommand: IRequest<AgregarUsuarioDto>
{
    public RegistroTokenDto TokenDto { get; set; }
}