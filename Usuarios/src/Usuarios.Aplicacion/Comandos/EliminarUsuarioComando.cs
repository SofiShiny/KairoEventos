using MediatR;

namespace Usuarios.Aplicacion.Comandos;

public record EliminarUsuarioComando : IRequest<Unit>
{
    public Guid UsuarioId { get; init; }
}
