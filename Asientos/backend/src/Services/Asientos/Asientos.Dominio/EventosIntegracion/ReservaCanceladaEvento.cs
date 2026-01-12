using System;

namespace Entradas.Dominio.Eventos;

/// <summary>
/// Evento espejo para recibir cancelaciones desde el microservicio de Entradas
/// </summary>
public record ReservaCanceladaEvento(
    Guid EntradaId,
    Guid? AsientoId,
    Guid EventoId,
    Guid UsuarioId,
    DateTime FechaCancelacion
);
