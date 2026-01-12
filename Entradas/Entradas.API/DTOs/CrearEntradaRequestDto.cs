using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entradas.API.DTOs;

/// <summary>
/// DTO para la solicitud de creación de una o múltiples entradas
/// </summary>
public class CrearEntradaRequestDto
{
    /// <summary>
    /// Identificador único del evento
    /// </summary>
    [Required(ErrorMessage = "El ID del evento es requerido")]
    public Guid EventoId { get; set; }

    /// <summary>
    /// Identificador único del usuario
    /// </summary>
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Lista de identificadores de asientos (para compra múltiple)
    /// </summary>
    public List<Guid>? AsientoIds { get; set; }

    /// <summary>
    /// Identificador único del asiento (para compatibilidad con compra individual)
    /// </summary>
    public Guid? AsientoId { get; set; }

    public List<string>? Cupones { get; set; }
}