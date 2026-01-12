using System;

namespace Entradas.Dominio.Eventos;

/// <summary>
/// Evento que se publica cuando una reserva de entrada es cancelada por expiración
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public record ReservaCanceladaEvento(
    Guid EntradaId,
    Guid? AsientoId,
    Guid EventoId,
    Guid UsuarioId,
    DateTime FechaCancelacion
);
