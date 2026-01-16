using System.Text.Json.Serialization;

namespace Comunidad.Application.DTOs;

public class ComentarioDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("foroId")]
    public Guid ForoId { get; set; }
    
    [JsonPropertyName("usuarioId")]
    public Guid UsuarioId { get; set; }
    
    [JsonPropertyName("contenido")]
    public string Contenido { get; set; } = string.Empty;
    
    [JsonPropertyName("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }
    
    [JsonPropertyName("respuestas")]
    public List<RespuestaDto> Respuestas { get; set; } = new();
}

public class RespuestaDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("usuarioId")]
    public Guid UsuarioId { get; set; }
    
    [JsonPropertyName("contenido")]
    public string Contenido { get; set; } = string.Empty;
    
    [JsonPropertyName("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }
}
