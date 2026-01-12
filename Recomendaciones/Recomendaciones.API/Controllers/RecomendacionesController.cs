using Microsoft.AspNetCore.Mvc;
using MediatR;
using Recomendaciones.Aplicacion.Queries;
using Recomendaciones.Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Recomendaciones.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecomendacionesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RecomendacionesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventoProyeccion>>> GetRecomendaciones()
    {
        var usuarioIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(usuarioIdStr)) return Unauthorized();
        
        var usuarioId = Guid.Parse(usuarioIdStr);

        var query = new ObtenerRecomendacionesQuery(usuarioId);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
}
