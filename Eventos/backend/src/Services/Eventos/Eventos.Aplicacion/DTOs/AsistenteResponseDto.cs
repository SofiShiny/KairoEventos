namespace Eventos.Aplicacion.DTOs;

//DTO específico para respuestas de API
//sin NombreUsuario (no se expone externamente)
public class AsistenteResponseDto
{
 public Guid Id { get; set; }
 public string Nombre { get; set; } = string.Empty;
 public string? Correo { get; set; }
 public DateTime RegistradoEn { get; set; }
}
