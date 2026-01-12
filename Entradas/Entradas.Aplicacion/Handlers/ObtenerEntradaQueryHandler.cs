using MediatR;
using Microsoft.Extensions.Logging;
using Entradas.Aplicacion.DTOs;
using Entradas.Aplicacion.Mappers;
using Entradas.Aplicacion.Queries;
using Entradas.Dominio.Interfaces;

namespace Entradas.Aplicacion.Handlers;

/// <summary>
/// Handler para procesar la query ObtenerEntradaQuery
/// </summary>
public class ObtenerEntradaQueryHandler : IRequestHandler<ObtenerEntradaQuery, EntradaDto?>
{
    private readonly IRepositorioEntradas _repositorio;
    private readonly ILogger<ObtenerEntradaQueryHandler> _logger;

    public ObtenerEntradaQueryHandler(
        IRepositorioEntradas repositorio,
        ILogger<ObtenerEntradaQueryHandler> logger)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EntradaDto?> Handle(ObtenerEntradaQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo entrada con ID {EntradaId}", request.EntradaId);

        try
        {
            var entrada = await _repositorio.ObtenerPorIdAsync(request.EntradaId, cancellationToken);
            
            if (entrada == null)
            {
                _logger.LogWarning("Entrada con ID {EntradaId} no encontrada", request.EntradaId);
                return null;
            }

            _logger.LogDebug("Entrada {EntradaId} encontrada exitosamente", entrada.Id);
            return EntradaMapper.ToDto(entrada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener entrada con ID {EntradaId}", request.EntradaId);
            throw;
        }
    }
}