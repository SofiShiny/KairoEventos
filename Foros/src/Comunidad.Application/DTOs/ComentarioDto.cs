namespace Comunidad.Application.DTOs;

public class ComentarioDto
{
    public Guid Id { get; set; }
    public Guid ForoId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public List<RespuestaDto> Respuestas { get; set; } = new();
}

public class RespuestaDto
{
    public Guid UsuarioId { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
