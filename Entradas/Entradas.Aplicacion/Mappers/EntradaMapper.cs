using Entradas.Aplicacion.Comandos;
using Entradas.Aplicacion.DTOs;
using Entradas.Dominio.Entidades;

namespace Entradas.Aplicacion.Mappers;

/// <summary>
/// Mapper estático para conversiones entre entidades de dominio y DTOs
/// </summary>
public static class EntradaMapper
{
    /// <summary>
    /// Convierte una entidad Entrada a EntradaDto
    /// </summary>
    /// <param name="entrada">Entidad de dominio</param>
    /// <returns>DTO de entrada</returns>
    public static EntradaDto ToDto(Entrada entrada)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        return new EntradaDto(
            entrada.Id,
            entrada.EventoId,
            entrada.UsuarioId,
            entrada.AsientoId,
            entrada.Monto,
            entrada.CodigoQr,
            entrada.Estado,
            entrada.FechaCompra,
            entrada.TituloEvento,
            $"{(entrada.NombreSector != null ? entrada.NombreSector + " - " : "")}Fila {entrada.Fila}, Asiento {entrada.NumeroAsiento}"
        );
    }

    /// <summary>
    /// Convierte una entidad Entrada a EntradaCreadaDto
    /// </summary>
    /// <param name="entrada">Entidad de dominio</param>
    /// <returns>DTO de entrada creada</returns>
    public static EntradaCreadaDto ToEntradaCreadaDto(Entrada entrada)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        return new EntradaCreadaDto(
            entrada.Id,
            entrada.EventoId,
            entrada.UsuarioId,
            entrada.AsientoId,
            entrada.Monto,
            entrada.CodigoQr,
            entrada.Estado,
            entrada.FechaCompra
        );
    }

    /// <summary>
    /// Convierte un CrearEntradaDto a CrearEntradaCommand
    /// </summary>
    /// <param name="dto">DTO de solicitud</param>
    /// <returns>Comando de aplicación</returns>
    public static CrearEntradaCommand ToCommand(CrearEntradaDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new CrearEntradaCommand(
            dto.EventoId,
            dto.UsuarioId,
            dto.AsientoId,
            dto.Cupones
        );
    }

    /// <summary>
    /// Convierte una lista de entidades Entrada a una lista de EntradaDto
    /// </summary>
    /// <param name="entradas">Lista de entidades de dominio</param>
    /// <returns>Lista de DTOs</returns>
    public static IEnumerable<EntradaDto> ToDto(IEnumerable<Entrada> entradas)
    {
        ArgumentNullException.ThrowIfNull(entradas);

        return entradas.Select(ToDto);
    }
}