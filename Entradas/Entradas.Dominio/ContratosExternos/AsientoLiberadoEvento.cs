namespace Asientos.Dominio.EventosDominio;

public record AsientoLiberadoEventoDominio(
    Guid MapaId,
    Guid AsientoId,
    int Fila,
    int Numero);
