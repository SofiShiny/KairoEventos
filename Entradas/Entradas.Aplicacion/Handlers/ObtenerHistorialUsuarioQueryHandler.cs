using MediatR;
using Microsoft.Extensions.Logging;
using Entradas.Aplicacion.DTOs;
using Entradas.Aplicacion.Queries;
using Entradas.Dominio.Interfaces;

namespace Entradas.Aplicacion.Handlers;

/// <summary>
/// Handler para obtener el historial completo de entradas de un usuario
/// </summary>
public class ObtenerHistorialUsuarioQueryHandler : IRequestHandler<ObtenerHistorialUsuarioQuery, List<EntradaResumenDto>>
{
    private readonly IRepositorioEntradas _repositorio;
    private readonly ILogger<ObtenerHistorialUsuarioQueryHandler> _logger;

    public ObtenerHistorialUsuarioQueryHandler(
        IRepositorioEntradas repositorio,
        ILogger<ObtenerHistorialUsuarioQueryHandler> logger)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<EntradaResumenDto>> Handle(
        ObtenerHistorialUsuarioQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo historial de entradas para usuario {UsuarioId}", request.UsuarioId);

        try
        {
            var entradas = await _repositorio.ObtenerPorUsuarioAsync(request.UsuarioId, cancellationToken);

            // Mapear entradas a DTOs de resumen con datos snapshot
            var historial = entradas
                .Where(e => e.Estado != Entradas.Dominio.Enums.EstadoEntrada.Cancelada)
                .OrderByDescending(e => e.FechaCompra)
                .Select(entrada => new EntradaResumenDto
                {
                    Id = entrada.Id,
                    EventoId = entrada.EventoId,
                    TituloEvento = entrada.TituloEvento,
                    EsVirtual = entrada.EsVirtual,
                    FechaEvento = entrada.FechaEvento,
                    AsientoId = entrada.AsientoId,
                    Sector = entrada.NombreSector,
                    Fila = entrada.Fila,
                    Numero = entrada.NumeroAsiento,
                    Estado = entrada.Estado,
                    MontoFinal = entrada.Monto,
                    CodigoQr = entrada.CodigoQr,
                    NombreUsuario = entrada.NombreUsuario,
                    EmailUsuario = entrada.EmailUsuario,
                    FechaCompra = entrada.FechaCompra,
                    FechaActualizacion = entrada.FechaActualizacion
                })
                .ToList();

            _logger.LogInformation(
                "Se encontraron {Cantidad} entradas para el usuario {UsuarioId}",
                historial.Count,
                request.UsuarioId);

            return historial;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener historial de entradas para usuario {UsuarioId}",
                request.UsuarioId);
            throw;
        }
    }
}
