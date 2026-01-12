using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Reportes.Dominio.ModelosLectura;

public class ReporteConsolidado
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("fechaConsolidacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaConsolidacion { get; set; } = DateTime.UtcNow;

    [BsonElement("periodoInicio")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime PeriodoInicio { get; set; }

    [BsonElement("periodoFin")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime PeriodoFin { get; set; }

    [BsonElement("totalIngresos")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalIngresos { get; set; }

    [BsonElement("totalReservas")]
    public int TotalReservas { get; set; }

    [BsonElement("totalEventos")]
    public int TotalEventos { get; set; }

    [BsonElement("promedioAsistenciaEvento")]
    public double PromedioAsistenciaEvento { get; set; }

    [BsonElement("ingresosPorCategoria")]
    public Dictionary<string, decimal> IngresosPorCategoria { get; set; } = new();
}
