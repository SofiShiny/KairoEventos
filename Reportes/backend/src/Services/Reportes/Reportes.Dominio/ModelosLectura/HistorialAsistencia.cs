using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Reportes.Dominio.ModelosLectura;

public class HistorialAsistencia
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("eventoId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid EventoId { get; set; }

    [BsonElement("tituloEvento")]
    public string TituloEvento { get; set; } = string.Empty;

    [BsonElement("totalAsistentesRegistrados")]
    public int TotalAsistentesRegistrados { get; set; }

    [BsonElement("capacidadTotal")]
    public int CapacidadTotal { get; set; }

    [BsonElement("asientosReservados")]
    public int AsientosReservados { get; set; }

    [BsonElement("asientosDisponibles")]
    public int AsientosDisponibles { get; set; }

    [BsonElement("porcentajeOcupacion")]
    public double PorcentajeOcupacion { get; set; }

    [BsonElement("asistentes")]
    public List<RegistroAsistente> Asistentes { get; set; } = new();

    [BsonElement("ultimaActualizacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;
}

public class RegistroAsistente
{
    [BsonElement("usuarioId")]
    public string UsuarioId { get; set; } = string.Empty;

    [BsonElement("nombreUsuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [BsonElement("fechaRegistro")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
