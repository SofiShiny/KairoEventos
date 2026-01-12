using MediatR;

namespace Usuarios.Aplicacion.Comandos;

public record CambiarPasswordComando : IRequest<bool>
{
    public Guid UsuarioId { get; init; }
    public string PasswordActual { get; init; } = string.Empty;
    public string NuevoPassword { get; init; } = string.Empty;
}
