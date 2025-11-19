using MediatR;
using Microsoft.AspNetCore.Mvc;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Aplicacion.DTOs;
using Swashbuckle.AspNetCore.Filters;
using Eventos.Aplicacion.Comandos;
using Eventos.Aplicacion.Queries;
using BloquesConstruccion.Aplicacion.Comun;

namespace Eventos.API.Controladores;

[ApiController]
[Route("api/[controller]")]
public class EventosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EventosController> _logger;

    public EventosController(IMediator mediator, ILogger<EventosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // v1 - solo GETs usando queries
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var resultado = await _mediator.Send(new ObtenerEventosQuery());
        if (resultado.EsFallido) return Problem(resultado.Error);
        return Ok(resultado.Valor);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        var resultado = await _mediator.Send(new ObtenerEventoPorIdQuery(id));
        if (resultado.EsFallido) return NotFound(resultado.Error);
        return Ok(resultado.Valor);
    }

    [HttpGet("organizador/{organizadorId}")]
    public async Task<IActionResult> ObtenerPorOrganizador(string organizadorId)
    {
        var resultado = await _mediator.Send(new ObtenerEventosPorOrganizadorQuery(organizadorId));
        if (resultado.EsFallido) return Problem(resultado.Error);
        return Ok(resultado.Valor);
    }

    [HttpGet("publicados")]
    public async Task<IActionResult> ObtenerPublicados()
    {
        var resultado = await _mediator.Send(new ObtenerEventosPublicadosQuery());
        if (resultado.EsFallido) return Problem(resultado.Error);
        return Ok(resultado.Valor);
    }

    // v2 - CRUD y acciones via comandos
    [HttpPost("v2")]
    [SwaggerRequestExample(typeof(EventoCreateDto), typeof(Eventos.API.Swagger.Examples.EventoCrearEjemploOperacion))]
    [ProducesResponseType(typeof(EventoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Crear([FromBody] EventoCreateDto dto)
    {
        try
        {
            if (dto == null) return BadRequest("Body requerido");
            if (dto.Ubicacion == null) return BadRequest("Ubicacion requerida");
            if (dto.FechaFin <= dto.FechaInicio) return BadRequest("FechaFin debe ser posterior a FechaInicio");

            var comando = new CrearEventoComando(
                dto.Titulo,
                dto.Descripcion ?? string.Empty,
                dto.Ubicacion,
                dto.FechaInicio,
                dto.FechaFin,
                dto.MaximoAsistentes,
                "organizador-001");

            var resultado = await _mediator.Send(comando);
            if (resultado.EsFallido) return BadRequest(resultado.Error);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor!.Id }, resultado.Valor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando evento");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("v2/{id}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] EventoUpdateDto dto)
    {
        try
        {
            var comando = new ActualizarEventoComando(
                id,
                dto.Titulo,
                dto.Descripcion,
                dto.Ubicacion,
                dto.FechaInicio,
                dto.FechaFin,
                dto.MaximoAsistentes);

            var resultado = await _mediator.Send(comando);
            if (resultado.EsFallido) return NotFound(resultado.Error);
            return Ok(resultado.Valor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando evento {Id}", id);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPatch("v2/{id}/publicar")]
    public async Task<IActionResult> Publicar(Guid id)
    {
        try
        {
            var resultado = await _mediator.Send(new PublicarEventoComando(id));
            if (resultado.EsFallido) return NotFound(resultado.Error);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publicando evento {Id}", id);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("v2/{id}/asistentes")]
    public async Task<IActionResult> RegistrarAsistente(Guid id, [FromBody] RegistrarAsistenteDto dto)
    {
        try
        {
            if (dto == null) return BadRequest("Body requerido");
            var comando = new RegistrarAsistenteComando(id, dto.UsuarioId, dto.Nombre, dto.Correo);
            var resultado = await _mediator.Send(comando);
            if (resultado.EsFallido) return BadRequest(resultado.Error);
            // Recuperar el evento actualizado para retornar el asistente recién agregado
            var eventoRes = await _mediator.Send(new ObtenerEventoPorIdQuery(id));
            if (eventoRes.EsFallido) return NotFound(eventoRes.Error);
            var asistente = eventoRes.Valor!.Asistentes!.Last();
            return Ok(asistente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registrando asistente en evento {Id}", id);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("v2/{id}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        try
        {
            var resultado = await _mediator.Send(new EliminarEventoComando(id));
            if (resultado.EsFallido) return NotFound(resultado.Error);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando evento {Id}", id);
            return StatusCode(500, ex.Message);
        }
    }
}
