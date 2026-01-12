using Streaming.Dominio.Modelos;

namespace Streaming.Aplicacion.DTOs;

public record TransmisionDto(
    Guid Id,
    Guid EventoId,
    PlataformaTransmision Plataforma,
    string UrlAcceso,
    EstadoTransmision Estado
);
