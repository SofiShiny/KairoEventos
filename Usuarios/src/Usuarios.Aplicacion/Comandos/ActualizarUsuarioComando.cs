using MediatR;

namespace Usuarios.Aplicacion.Comandos;

public record ActualizarUsuarioComando : IRequest<Unit>
{
    public Guid UsuarioId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
}
