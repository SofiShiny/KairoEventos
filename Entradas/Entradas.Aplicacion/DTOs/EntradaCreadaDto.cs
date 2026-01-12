using Entradas.Dominio.Enums;
using System.Text.Json.Serialization;

namespace Entradas.Aplicacion.DTOs;

/// <summary>
/// DTO que representa una entrada recién creada en el sistema
/// </summary>
public record EntradaCreadaDto(
    Guid Id,
    Guid EventoId,
    Guid UsuarioId,
    Guid? AsientoId,
    
    [property: JsonPropertyName("precio")]
    decimal Monto,
    
    string CodigoQr,
    
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    EstadoEntrada Estado,
    
    DateTime FechaCompra
);