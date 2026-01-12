using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Reportes.Dominio.ModelosLectura;

public class MetricasEvento
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("eventoId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid EventoId { get; set; }

    [BsonElement("tituloEvento")]
    public string TituloEvento { get; set; } = string.Empty;

    [BsonElement("fechaInicio")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaInicio { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = "Publicado"; // Publicado, Cancelado, Finalizado

    [BsonElement("totalAsistentes")]
    public int TotalAsistentes { get; set; }

    [BsonElement("totalReservas")]
    public int TotalReservas { get; set; }

    [BsonElement("ingresoTotal")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal IngresoTotal { get; set; }

    [BsonElement("totalDescuentos")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalDescuentos { get; set; }

    [BsonElement("usoDeCupones")]
    public Dictionary<string, int> UsoDeCupones { get; set; } = new();

    [BsonElement("fechaCreacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [BsonElement("ultimaActualizacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;
}
