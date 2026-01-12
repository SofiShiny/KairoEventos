using Microsoft.AspNetCore.Mvc;
using MediatR;
using Entradas.Aplicacion.Comandos;
using Entradas.Aplicacion.Queries;
using Entradas.Aplicacion.DTOs;
using Entradas.Dominio.Excepciones;
using Entradas.API.DTOs;

namespace Entradas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EntradasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EntradasController> _logger;

    public EntradasController(IMediator mediator, ILogger<EntradasController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> CrearEntrada(
        [FromBody] CrearEntradaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Determinar los asientos a procesar (compatibilidad con ambos formatos)
            var asientosAProcesar = new List<Guid?>();
            
            if (request.AsientoIds != null && request.AsientoIds.Any())
            {
                // Compra múltiple
                asientosAProcesar.AddRange(request.AsientoIds.Cast<Guid?>());
            }
            else if (request.AsientoId.HasValue)
            {
                // Compra individual (compatibilidad)
                asientosAProcesar.Add(request.AsientoId);
            }
            else
            {
                // Entrada general sin asiento
                asientosAProcesar.Add(null);
            }

            var entradasCreadas = new List<EntradaCreadaDto>();
            decimal montoTotal = 0;

            // Crear una entrada por cada asiento
            foreach (var asientoId in asientosAProcesar)
            {
                var comando = new CrearEntradaCommand(
                    request.EventoId,
                    request.UsuarioId,
                    asientoId,
                    request.Cupones);

                var resultado = await _mediator.Send(comando, cancellationToken);
                entradasCreadas.Add(resultado);
                montoTotal += resultado.Monto;
            }

            // Si es compra múltiple, retornar resumen consolidado
            if (entradasCreadas.Count > 1)
            {
                var resumenMultiple = new
                {
                    entradas = entradasCreadas,
                    montoTotal = montoTotal,
                    cantidad = entradasCreadas.Count,
                    ordenId = entradasCreadas.First().Id // Usar el ID de la primera entrada como orden
                };

                return Ok(ApiResponse<object>.Ok(resumenMultiple));
            }

            // Compra individual - retornar formato original
            return CreatedAtAction(nameof(ObtenerEntrada), 
                new { id = entradasCreadas.First().Id }, 
                ApiResponse<EntradaCreadaDto>.Ok(entradasCreadas.First()));
        }
        catch (EventoNoDisponibleException ex)
        {
            return NotFound(new ProblemDetails { Title = "Evento no disponible", Detail = ex.Message });
        }
        catch (AsientoNoDisponibleException ex)
        {
            return Conflict(new ProblemDetails { Title = "Asiento no disponible", Detail = ex.Message });
        }
        catch (DominioException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Error de validación", Detail = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EntradaDto>>> ObtenerEntrada(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new ObtenerEntradaQuery(id);
        var entrada = await _mediator.Send(query, cancellationToken);
        if (entrada == null) return NotFound(new ProblemDetails { Title = "No encontrado" });
        return Ok(ApiResponse<EntradaDto>.Ok(entrada));
    }

    [HttpGet("mis-entradas")]
    public async Task<ActionResult<ApiResponse<List<EntradaResumenDto>>>> ObtenerMisEntradas(
        [FromQuery] Guid? usuarioId,
        CancellationToken cancellationToken = default)
    {
        if (!usuarioId.HasValue && Request.Headers.TryGetValue("X-User-Id", out var userIdStr))
        {
            if (Guid.TryParse(userIdStr, out var id)) usuarioId = id;
        }

        if (!usuarioId.HasValue)
        {
            return Ok(ApiResponse<List<EntradaResumenDto>>.Ok(new List<EntradaResumenDto>()));
        }

        var query = new ObtenerHistorialUsuarioQuery(usuarioId.Value);
        var historial = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<List<EntradaResumenDto>>.Ok(historial));
    }

    [HttpGet("usuario/{usuarioId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<EntradaDto>>>> ObtenerEntradasPorUsuario(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);
        var entradas = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<IEnumerable<EntradaDto>>.Ok(entradas));
    }

    [HttpGet("historial/{usuarioId:guid}")]
    public async Task<ActionResult<ApiResponse<List<EntradaResumenDto>>>> ObtenerHistorialUsuario(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var query = new ObtenerHistorialUsuarioQuery(usuarioId);
        var historial = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<List<EntradaResumenDto>>.Ok(historial));
    }

    [HttpGet("todas")]
    public async Task<ActionResult<ApiResponse<IEnumerable<EntradaDto>>>> ObtenerTodas(
        [FromQuery] Guid? organizadorId,
        CancellationToken cancellationToken = default)
    {
        var query = new ObtenerTodasLasEntradasQuery(organizadorId);
        var entradas = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<IEnumerable<EntradaDto>>.Ok(entradas));
    }

    [HttpGet("health")]
    public ActionResult Health() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}