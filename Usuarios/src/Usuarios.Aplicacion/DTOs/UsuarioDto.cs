using Usuarios.Dominio.Enums;

namespace Usuarios.Aplicacion.DTOs;

public record UsuarioDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
    public Rol Rol { get; init; }
    public DateTime FechaCreacion { get; init; }
    
    // Campos adicionales para compatibilidad con frontend de admin
    public string Email => Correo;
    public List<string> Roles => new List<string> { Rol.ToString().ToLower() };
    public bool Enabled { get; init; } = true;
    public long CreatedTimestamp => ((DateTimeOffset)FechaCreacion).ToUnixTimeMilliseconds();
}
