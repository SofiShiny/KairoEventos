using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Entradas.Dominio.Interfaces;

/// <summary>
/// Interface para verificar la existencia y disponibilidad de eventos
/// </summary>
public interface IVerificadorEventos
{
    /// <summary>
    /// Verifica si un evento existe y está disponible para la venta de entradas
    /// </summary>
    /// <param name="eventoId">ID del evento a verificar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el evento existe y está disponible, false en caso contrario</returns>
    Task<bool> EventoExisteYDisponibleAsync(Guid eventoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene información básica de un evento
    /// </summary>
    /// <param name="eventoId">ID del evento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del evento o null si no existe</returns>
    Task<EventoInfo?> ObtenerInfoEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene sugerencias de eventos basados en categoría
    /// </summary>
    /// <param name="categoria">Categoría de interés</param>
    /// <param name="cantidad">Cantidad de eventos a retornar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de eventos sugeridos</returns>
    Task<IEnumerable<EventoRecomendado>> ObtenerSugerenciasAsync(string? categoria, int cantidad, CancellationToken cancellationToken = default);
}

/// <summary>
/// Información resumida para recomendaciones
/// </summary>
public record EventoRecomendado(
    Guid Id,
    string Titulo,
    string? Categoria,
    DateTime FechaInicio,
    string? UrlImagen
);

/// <summary>
/// Información básica de un evento
/// </summary>
public record EventoInfo(
    Guid Id,
    string Nombre,
    DateTime FechaEvento,
    bool EstaDisponible,
    decimal PrecioBase,
    string? UrlImagen = null,
    string? OrganizadorId = null,
    bool EsVirtual = false
);