using MediatR;
using Microsoft.Extensions.Logging;
using Entradas.Aplicacion.DTOs;
using Entradas.Aplicacion.Mappers;
using Entradas.Aplicacion.Queries;
using Entradas.Dominio.Interfaces;

namespace Entradas.Aplicacion.Handlers;

/// <summary>
/// Handler para procesar la query ObtenerEntradasPorUsuarioQuery
/// </summary>
public class ObtenerEntradasPorUsuarioQueryHandler : IRequestHandler<ObtenerEntradasPorUsuarioQuery, IEnumerable<EntradaDto>>
{
    private readonly IRepositorioEntradas _repositorio;
    private readonly ILogger<ObtenerEntradasPorUsuarioQueryHandler> _logger;

    public ObtenerEntradasPorUsuarioQueryHandler(
        IRepositorioEntradas repositorio,
        ILogger<ObtenerEntradasPorUsuarioQueryHandler> logger)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<EntradaDto>> Handle(ObtenerEntradasPorUsuarioQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo entradas para usuario {UsuarioId}", request.UsuarioId);

        try
        {
            var entradas = await _repositorio.ObtenerPorUsuarioAsync(request.UsuarioId, cancellationToken);
            
            // Devolvemos todas las entradas no canceladas
            var entradasFiltradas = entradas.Where(e => 
                e.Estado != Entradas.Dominio.Enums.EstadoEntrada.Cancelada);

            var entradasDto = EntradaMapper.ToDto(entradasFiltradas).ToList();
            
            _logger.LogInformation("Se encontraron {Count} entradas para usuario {UsuarioId}", 
                entradasDto.Count, request.UsuarioId);
            
            return entradasDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener entradas para usuario {UsuarioId}", request.UsuarioId);
            throw;
        }
    }
}