namespace Usuarios.Aplicacion.DTOs;

public record CambiarPasswordDto
{
    public string PasswordActual { get; init; } = string.Empty;
    public string NuevoPassword { get; init; } = string.Empty;
}
