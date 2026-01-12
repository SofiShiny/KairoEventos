using Comunidad.Application.Comandos;
using Comunidad.Application.Consultas;
using Comunidad.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Comunidad.API.Controllers;

[ApiController]
[Route("api/comunidad")]
public class ComentariosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ComentariosController> _logger;

    public ComentariosController(IMediator mediator, ILogger<ComentariosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener comentarios visibles de un foro por EventoId
    /// </summary>
    [HttpGet("foros/{eventoId:guid}")]
    [ProducesResponseType(typeof(List<ComentarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ComentarioDto>>> ObtenerComentarios(Guid eventoId)
    {
        var query = new ObtenerComentariosQuery(eventoId);
        var comentarios = await _mediator.Send(query);
        return Ok(comentarios);
    }

    /// <summary>
    /// Crear un nuevo comentario principal
    /// </summary>
    [HttpPost("comentarios")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CrearComentario([FromBody] CrearComentarioRequest request)
    {
        try
        {
            var comando = new CrearComentarioComando(
                request.ForoId,
                request.UsuarioId,
                request.Contenido);

            var comentarioId = await _mediator.Send(comando);

            return CreatedAtAction(
                nameof(ObtenerComentarios),
                new { eventoId = request.ForoId },
                comentarioId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al crear comentario");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Responder a un comentario existente
    /// </summary>
    [HttpPost("comentarios/{id:guid}/responder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResponderComentario(Guid id, [FromBody] ResponderComentarioRequest request)
    {
        try
        {
            var comando = new ResponderComentarioComando(
                id,
                request.UsuarioId,
                request.Contenido);

            await _mediator.Send(comando);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al responder comentario");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Ocultar un comentario (moderación)
    /// </summary>
    [HttpDelete("comentarios/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OcultarComentario(Guid id)
    {
        try
        {
            var comando = new OcultarComentarioComando(id);
            await _mediator.Send(comando);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al ocultar comentario");
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record CrearComentarioRequest(Guid ForoId, Guid UsuarioId, string Contenido);
public record ResponderComentarioRequest(Guid UsuarioId, string Contenido);
