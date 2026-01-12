using Microsoft.AspNetCore.Mvc;
using MediatR;
using Encuestas.Aplicacion.Comandos;
using Encuestas.Dominio.Repositorios;

namespace Encuestas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EncuestasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRepositorioEncuestas _repositorio;

    public EncuestasController(IMediator mediator, IRepositorioEncuestas repositorio)
    {
        _mediator = mediator;
        _repositorio = repositorio;
    }

    [HttpGet("evento/{eventoId:guid}")]
    public async Task<ActionResult> GetEncuestaPorEvento(Guid eventoId)
    {
        var encuesta = await _repositorio.ObtenerPorEventoIdAsync(eventoId);
        if (encuesta == null) return NotFound();
        return Ok(encuesta);
    }

    [HttpPost("responder")]
    public async Task<ActionResult<Guid>> Responder([FromBody] ResponderEncuestaCommand command)
    {
        try
        {
            var id = await _mediator.Send(command);
            return StatusCode(201, id);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/publicar")]
    public async Task<ActionResult> Publicar(Guid id)
    {
        await _mediator.Send(new PublicarEncuestaCommand(id));
        return NoContent();
    }
}
