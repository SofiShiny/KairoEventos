using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Comunidad.Domain.Entidades;

[BsonIgnoreExtraElements]
public class Comentario
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("foroId")]
    [BsonRepresentation(BsonType.String)]
    public Guid ForoId { get; set; }

    [BsonElement("usuarioId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UsuarioId { get; set; }

    [BsonElement("contenido")]
    public string Contenido { get; set; } = string.Empty;

    [BsonElement("esVisible")]
    [BsonDefaultValue(true)]
    public bool EsVisible { get; set; } = true;

    [BsonElement("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }

    [BsonElement("respuestas")]
    public List<Respuesta> Respuestas { get; set; }

    public Comentario()
    {
        Id = Guid.NewGuid();
        EsVisible = true;
        FechaCreacion = DateTime.UtcNow;
        Respuestas = new List<Respuesta>();
    }

    public static Comentario Crear(Guid foroId, Guid usuarioId, string contenido)
    {
        return new Comentario
        {
            ForoId = foroId,
            UsuarioId = usuarioId,
            Contenido = contenido
        };
    }

    public void AgregarRespuesta(Guid usuarioId, string contenido)
    {
        Respuestas.Add(new Respuesta
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Contenido = contenido,
            FechaCreacion = DateTime.UtcNow,
            EsVisible = true
        });
    }

    public void Ocultar()
    {
        EsVisible = false;
    }

    public void OcultarRespuesta(Guid respuestaId)
    {
        var respuesta = Respuestas.FirstOrDefault(r => r.Id == respuestaId);
        if (respuesta != null)
        {
            respuesta.EsVisible = false;
        }
        else
        {
            // Fallback determinista para datos antiguos sin ID
            var idStr = respuestaId.ToString().ToLower();
            if (idStr.StartsWith("00000000-0000-0000-0000-"))
            {
                var sub = idStr.Substring(24);
                if (int.TryParse(sub, out int idx) && idx >= 0 && idx < Respuestas.Count)
                {
                    Respuestas[idx].EsVisible = false;
                }
            }
        }
    }
}

[BsonIgnoreExtraElements]
public class Respuesta
{
    [BsonElement("id")]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("usuarioId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UsuarioId { get; set; }

    [BsonElement("contenido")]
    public string Contenido { get; set; } = string.Empty;

    [BsonElement("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }

    [BsonElement("esVisible")]
    [BsonDefaultValue(true)]
    public bool EsVisible { get; set; } = true;

    public Respuesta()
    {
        Id = Guid.NewGuid();
        EsVisible = true;
    }
}
