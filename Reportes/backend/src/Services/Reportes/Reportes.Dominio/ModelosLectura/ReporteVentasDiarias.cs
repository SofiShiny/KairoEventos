using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Reportes.Dominio.ModelosLectura;

public class ReporteVentasDiarias
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("fecha")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Fecha { get; set; }

    [BsonElement("eventoId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid EventoId { get; set; }

    [BsonElement("tituloEvento")]
    public string TituloEvento { get; set; } = string.Empty;

    [BsonElement("cantidadReservas")]
    public int CantidadReservas { get; set; }

    [BsonElement("totalIngresos")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalIngresos { get; set; }

    [BsonElement("reservasPorCategoria")]
    public Dictionary<string, int> ReservasPorCategoria { get; set; } = new();

    [BsonElement("ultimaActualizacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;
}
