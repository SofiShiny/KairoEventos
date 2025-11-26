namespace Eventos.Aplicacion.DTOs;

// AsistenteDto: DTO general usado internamente en la aplicación
// Incluye NombreUsuario para uso interno (mapeo desde entidad de dominio)
public class AsistenteDto
{
    public Guid Id { get; set; }
    public string? NombreUsuario { get; set; }
    public string? Nombre { get; set; }
    public string? Correo { get; set; }
    public DateTime RegistradoEn { get; set; }
}
