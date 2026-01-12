using System;
using System.Collections.Generic;
using MediatR;
using Entradas.Aplicacion.DTOs;

namespace Entradas.Aplicacion.Comandos;

/// <summary>
/// Comando para crear una nueva entrada en el sistema con snapshot de datos descriptivos
/// </summary>
/// <param name="EventoId">Identificador único del evento</param>
/// <param name="UsuarioId">Identificador único del usuario</param>
/// <param name="AsientoId">Identificador único del asiento (opcional para entradas generales)</param>
public record CrearEntradaCommand(
    Guid EventoId,
    Guid UsuarioId,
    Guid? AsientoId,
    List<string>? Cupones = null,
    string? TituloEvento = null,
    string? ImagenEventoUrl = null,
    string? Categoria = null,
    DateTime? FechaEvento = null,
    string? NombreSector = null,
    string? Fila = null,
    int? NumeroAsiento = null,
    string? NombreUsuario = null,
    string? Email = null
) : IRequest<EntradaCreadaDto>;