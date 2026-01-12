namespace Eventos.Domain.Events;

// Contrato espejo para recibir eventos de el microservicio de Eventos
public record EventoPublicadoEventoDominio(
    Guid EventoId,
    string Nombre,
    DateTime Fecha,
    bool EsOnline,
    decimal PrecioBase
);
