namespace Usuarios.Application.Dtos;

public class ConsultarPerfilUsuarioPorIdDto
{
    public required string Nombre { get; set; }
    public required string Correo { get; set; }
    public required string Telefono { get; set; }
    public required string Direccion { get; set; }
}