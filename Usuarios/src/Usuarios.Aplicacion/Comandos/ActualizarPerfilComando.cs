using MediatR;

namespace Usuarios.Aplicacion.Comandos;

/// <summary>
/// Comando para actualizar el perfil del usuario
/// </summary>
public class ActualizarPerfilComando : IRequest<bool>
{
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}
