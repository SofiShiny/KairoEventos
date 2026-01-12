// IMPORTANTE: Este archivo usa el namespace del microservicio de origen (Asientos)
// para que MassTransit pueda deserializar correctamente los eventos publicados.
// NO cambiar el namespace a Reportes.Dominio.*

namespace Asientos.Dominio.EventosDominio;

/// <summary>
/// Contrato espejo del evento MapaAsientosCreadoEventoDominio del microservicio de Asientos.
/// Se publica cuando se crea un mapa de asientos para un evento.
/// </summary>
public record MapaAsientosCreadoEventoDominio
{
    public Guid MapaId { get; init; }
    public Guid EventoId { get; init; }
}

/// <summary>
/// Contrato espejo del evento CategoriaAgregadaEventoDominio del microservicio de Asientos.
/// Se publica cuando se agrega una categoría de asientos al mapa.
/// </summary>
public record CategoriaAgregadaEventoDominio
{
    public Guid MapaId { get; init; }
    public string NombreCategoria { get; init; } = string.Empty;
}

/// <summary>
/// Contrato espejo del evento AsientoAgregadoEventoDominio del microservicio de Asientos.
/// Se publica cuando se agrega un asiento al mapa.
/// </summary>
public record AsientoAgregadoEventoDominio
{
    public Guid MapaId { get; init; }
    public int Fila { get; init; }
    public int Numero { get; init; }
    public string Categoria { get; init; } = string.Empty;
}

/// <summary>
/// Contrato espejo del evento AsientoReservadoEventoDominio del microservicio de Asientos.
/// Se publica cuando un asiento es reservado por un usuario.
/// </summary>
public record AsientoReservadoEventoDominio
{
    public Guid MapaId { get; init; }
    public int Fila { get; init; }
    public int Numero { get; init; }
}

/// <summary>
/// Contrato espejo del evento AsientoLiberadoEventoDominio del microservicio de Asientos.
/// Se publica cuando un asiento reservado es liberado.
/// </summary>
public record AsientoLiberadoEventoDominio
{
    public Guid MapaId { get; init; }
    public int Fila { get; init; }
    public int Numero { get; init; }
}
