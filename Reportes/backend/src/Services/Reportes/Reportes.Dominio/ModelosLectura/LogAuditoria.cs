using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Reportes.Dominio.ModelosLectura;

public class LogAuditoria
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("timestamp")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("tipoOperacion")]
    public string TipoOperacion { get; set; } = string.Empty; // EventoConsumido, ReporteGenerado, ErrorProcesamiento

    [BsonElement("entidad")]
    public string Entidad { get; set; } = string.Empty;

    [BsonElement("entidadId")]
    public string EntidadId { get; set; } = string.Empty;

    [BsonElement("detalles")]
    public string Detalles { get; set; } = string.Empty;

    [BsonElement("usuario")]
    public string Usuario { get; set; } = "Sistema";

    [BsonElement("exitoso")]
    public bool Exitoso { get; set; }

    [BsonElement("mensajeError")]
    public string? MensajeError { get; set; }
}
