using System.ComponentModel.DataAnnotations;

namespace Usuarios.Aplicacion.DTOs;

/// <summary>
/// DTO para actualizar el perfil del usuario
/// </summary>
public class ActualizarPerfilDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Direccion { get; set; }
}
