using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Comunidad.Domain.Entidades;

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
    public bool EsVisible { get; set; }

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
            UsuarioId = usuarioId,
            Contenido = contenido,
            FechaCreacion = DateTime.UtcNow
        });
    }

    public void Ocultar()
    {
        EsVisible = false;
    }
}

public class Respuesta
{
    [BsonElement("usuarioId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UsuarioId { get; set; }

    [BsonElement("contenido")]
    public string Contenido { get; set; } = string.Empty;

    [BsonElement("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }
}
