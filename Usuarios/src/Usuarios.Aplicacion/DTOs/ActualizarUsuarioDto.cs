namespace Usuarios.Aplicacion.DTOs;

public record ActualizarUsuarioDto
{
    public string Nombre { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
}
