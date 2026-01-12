using Entradas.Dominio.Enums;
using System.Text.Json.Serialization;

namespace Entradas.Aplicacion.DTOs;

/// <summary>
/// DTO que representa una entrada existente en el sistema
/// compatible con el frontend unificado
/// </summary>
public record EntradaDto(
    Guid Id,
    Guid EventoId,
    Guid UsuarioId,
    Guid? AsientoId,
    
    [property: JsonPropertyName("precio")]
    decimal Monto,
    
    string CodigoQr,
    
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    EstadoEntrada Estado,
    
    DateTime FechaCompra,
    
    [property: JsonPropertyName("eventoNombre")]
    string? EventoNombre = "Evento",
    
    [property: JsonPropertyName("asientoInfo")]
    string? AsientoInfo = "General"
);