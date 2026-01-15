using Microsoft.AspNetCore.Mvc;
using MediatR;
using Recomendaciones.Aplicacion.Queries;
using Recomendaciones.Aplicacion.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Recomendaciones.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    /// Obtiene los eventos más populares (tendencias)
    /// </summary>
    /// <param name="limite">Número de eventos a retornar (default: 5)</param>
    /// <returns>Lista de eventos tendencia</returns>
    [HttpGet("tendencias")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<EventoRecomendadoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EventoRecomendadoDto>>> GetTendencias([FromQuery] int limite = 5)
    {
        _logger.LogInformation("Solicitando tendencias - Límite: {Limite}", limite);
        
        var query = new ObtenerTendenciasQuery(limite);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    /// <summary>
    /// Obtiene recomendaciones personalizadas para un usuario
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <returns>Recomendaciones personalizadas basadas en historial</returns>
    [HttpGet("usuario/{usuarioId}")]
    [AllowAnonymous] // Permitir acceso sin autenticación para facilitar pruebas
    [ProducesResponseType(typeof(RecomendacionesPersonalizadasDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecomendacionesPersonalizadasDto>> GetRecomendacionesUsuario(Guid usuarioId)
    {
        _logger.LogInformation("Solicitando recomendaciones para usuario {UsuarioId}", usuarioId);
        
        var query = new ObtenerRecomendacionesPersonalizadasQuery(usuarioId);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    /// <summary>
    /// Obtiene recomendaciones para el usuario autenticado
    /// </summary>
    /// <returns>Recomendaciones personalizadas</returns>
    [HttpGet("mi-perfil")]
    [Authorize]
    [ProducesResponseType(typeof(RecomendacionesPersonalizadasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RecomendacionesPersonalizadasDto>> GetMisRecomendaciones()
    {
        var usuarioIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        
        if (string.IsNullOrEmpty(usuarioIdStr))
        {
            _logger.LogWarning("Usuario no autenticado intentó acceder a recomendaciones personalizadas");
            return Unauthorized();
        }
        
        var usuarioId = Guid.Parse(usuarioIdStr);
        _logger.LogInformation("Usuario autenticado {UsuarioId} solicitando sus recomendaciones", usuarioId);
        
        var query = new ObtenerRecomendacionesPersonalizadasQuery(usuarioId);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
}
