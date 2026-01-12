using MediatR;
using Microsoft.AspNetCore.Mvc;
using Entradas.Aplicacion.Queries;
using Entradas.Aplicacion.DTOs;

namespace Entradas.API.Controllers;

[ApiController]
[Route("api/entradas/recomendaciones")]
public class RecomendacionesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecomendacionesController> _logger;

    public RecomendacionesController(IMediator mediator, ILogger<RecomendacionesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene recomendaciones de eventos basadas en el historial del usuario
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="cantidad">Cantidad de recomendaciones</param>
    [HttpGet("{usuarioId}")]
    [ProducesResponseType(typeof(IEnumerable<EventoRecomendadoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventoRecomendadoDto>>> GetRecomendaciones(Guid usuarioId, [FromQuery] int cantidad = 3)
    {
        _logger.LogInformation("Endpoint de recomendaciones invocado para usuario {UsuarioId}", usuarioId);
        
        var query = new ObtenerRecomendacionesQuery(usuarioId, cantidad);
        var resultado = await _mediator.Send(query);
        
        return Ok(resultado);
    }
}
