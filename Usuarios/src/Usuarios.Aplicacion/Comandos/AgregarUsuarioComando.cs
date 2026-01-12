using MediatR;
using Usuarios.Dominio.Enums;

namespace Usuarios.Aplicacion.Comandos;

public record AgregarUsuarioComando : IRequest<Guid>
{
    public string Username { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
    public Rol Rol { get; init; }
    public string Password { get; init; } = string.Empty;
}
