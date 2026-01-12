using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Comunidad.Domain.Entidades;

public class Foro
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("eventoId")]
    [BsonRepresentation(BsonType.String)]
    public Guid EventoId { get; set; }

    [BsonElement("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [BsonElement("fechaCreacion")]
    public DateTime FechaCreacion { get; set; }

    public Foro()
    {
        Id = Guid.NewGuid();
        FechaCreacion = DateTime.UtcNow;
    }

    public static Foro Crear(Guid eventoId, string titulo)
    {
        return new Foro
        {
            EventoId = eventoId,
            Titulo = titulo
        };
    }
}
