using Entradas.Dominio.Enums;
using System.Text.Json.Serialization;

namespace Entradas.Aplicacion.DTOs;

/// <summary>
/// DTO que representa un resumen de entrada para el historial del usuario
/// Incluye información enriquecida del evento y asiento
/// </summary>
public record EntradaResumenDto
{
    public Guid Id { get; init; }
    public Guid EventoId { get; init; }
    
    // Alias para compatibilidad con el frontend unificado
    [JsonPropertyName("eventoNombre")]
    public string? TituloEvento { get; init; }
    
    public DateTime? FechaEvento { get; init; }
    public Guid? AsientoId { get; init; }
    
    // Campos individuales originales
    public string? Sector { get; init; }
    public string? Fila { get; init; }
    public int? Numero { get; init; }
    
    // Campo compuesto para compatibilidad con frontend
    [JsonPropertyName("asientoInfo")]
    public string AsientoInfo => $"{(Sector != null ? Sector + " - " : "")}Fila {Fila}, Asiento {Numero}";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EstadoEntrada Estado { get; init; }
    
    // Alias 'precio' para compatibilidad con frontend
    [JsonPropertyName("precio")]
    public decimal MontoFinal { get; init; }

    public string CodigoQr { get; init; } = string.Empty;
    public DateTime FechaCompra { get; init; }
    public DateTime FechaActualizacion { get; init; }
}
