using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Reportes.Dominio.ModelosLectura;

public class MetricasDiarias
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("fecha")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Fecha { get; set; }

    [BsonElement("totalVentas")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalVentas { get; set; }

    [BsonElement("entradasVendidas")]
    public int EntradasVendidas { get; set; }

    [BsonElement("eventoId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid EventoId { get; set; }
}
