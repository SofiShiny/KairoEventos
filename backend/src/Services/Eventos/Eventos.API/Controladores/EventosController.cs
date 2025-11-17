File: backend\src\Services\Eventos\Eventos.API\Controladores\EventosController.cs
````````csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Aplicacion.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace Eventos.API.Controladores;

[ApiController]
[Route("api/[controller]")]
public class EventosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRepositorioEvento _repositorio;
    private readonly ILogger<EventosController> _logger;

    public EventosController(IMediator mediator, IRepositorioEvento repositorio, ILogger<EventosController> logger)
    {
        _mediator = mediator;
        _repositorio = repositorio;
        _logger = logger;
    }

    // v1 - solo GETs
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var eventos = await _repositorio.ObtenerTodosAsync();
        var dtos = eventos.Select(MapToDto);
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        var evento = await _repositorio.ObtenerPorIdAsync(id);
        if (evento == null)
            return NotFound();
        return Ok(MapToDto(evento));
    }

    [HttpGet("organizador/{organizadorId}")]
    public async Task<IActionResult> ObtenerPorOrganizador(string organizadorId)
    {
        var eventos = await _repositorio.ObtenerEventosPorOrganizadorAsync(organizadorId);
        return Ok(eventos.Select(MapToDto));
    }

    [HttpGet("publicados")]
    public async Task<IActionResult> ObtenerPublicados()
    {
        var eventos = await _repositorio.ObtenerEventosPublicadosAsync();
        return Ok(eventos.Select(MapToDto));
    }

    // v2 - CRUD y acciones
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

            var ubic = new Ubicacion(dto.Ubicacion.NombreLugar ?? string.Empty,
                dto.Ubicacion.Direccion ?? string.Empty,
                dto.Ubicacion.Ciudad ?? string.Empty,
                dto.Ubicacion.Region ?? string.Empty,
                dto.Ubicacion.CodigoPostal ?? string.Empty,
                dto.Ubicacion.Pais ?? string.Empty);

            var evento = new Evento(dto.Titulo, dto.Descripcion ?? string.Empty, ubic, dto.FechaInicio, dto.FechaFin, dto.MaximoAsistentes, "organizador-001");
            await _repositorio.AgregarAsync(evento);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = evento.Id }, MapToDto(evento));
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
            var evento = await _repositorio.ObtenerPorIdAsync(id);
            if (evento == null) return NotFound();

            var nuevaUbic = dto.Ubicacion is null ? evento.Ubicacion : new Ubicacion(
                dto.Ubicacion!.NombreLugar ?? evento.Ubicacion.NombreLugar,
                dto.Ubicacion!.Direccion ?? evento.Ubicacion.Direccion,
                dto.Ubicacion!.Ciudad ?? evento.Ubicacion.Ciudad,
                dto.Ubicacion!.Region ?? evento.Ubicacion.Region,
                dto.Ubicacion!.CodigoPostal ?? evento.Ubicacion.CodigoPostal,
                dto.Ubicacion!.Pais ?? evento.Ubicacion.Pais);

            evento.Actualizar(
                dto.Titulo ?? evento.Titulo,
                dto.Descripcion ?? evento.Descripcion,
                nuevaUbic,
                dto.FechaInicio ?? evento.FechaInicio,
                dto.FechaFin ?? evento.FechaFin,
                dto.MaximoAsistentes ?? evento.MaximoAsistentes);

            await _repositorio.ActualizarAsync(evento);
            return Ok(MapToDto(evento));
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
            var evento = await _repositorio.ObtenerPorIdAsync(id);
            if (evento == null) return NotFound();
            evento.Publicar();
            await _repositorio.ActualizarAsync(evento);
            return Ok(MapToDto(evento));
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

            var evento = await _repositorio.ObtenerPorIdAsync(id);
            if (evento == null) return NotFound();
            evento.RegistrarAsistente(dto.UsuarioId, dto.Nombre, dto.Correo);
            await _repositorio.ActualizarAsync(evento);
            var asistente = evento.Asistentes.Last();
            return Ok(new AsistenteDto
            {
                Id = asistente.Id,
                NombreUsuario = asistente.NombreUsuario,
                Correo = asistente.Correo,
                RegistradoEn = asistente.RegistradoEn
            });
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
            var evento = await _repositorio.ObtenerPorIdAsync(id);
            if (evento == null) return NotFound();
            await _repositorio.EliminarAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando evento {Id}", id);
            return StatusCode(500, ex.Message);
        }
    }

    private static EventoDto MapToDto(Evento evento) => new()
    {
        Id = evento.Id,
        Titulo = evento.Titulo,
        Descripcion = evento.Descripcion,
        Ubicacion = new UbicacionDto
        {
            NombreLugar = evento.Ubicacion.NombreLugar,
            Direccion = evento.Ubicacion.Direccion,
            Ciudad = evento.Ubicacion.Ciudad,
            Region = evento.Ubicacion.Region,
            CodigoPostal = evento.Ubicacion.CodigoPostal,
            Pais = evento.Ubicacion.Pais
        },
        FechaInicio = evento.FechaInicio,
        FechaFin = evento.FechaFin,
        MaximoAsistentes = evento.MaximoAsistentes,
        ConteoAsistentesActual = evento.ConteoAsistentesActual,
        Estado = evento.Estado.ToString(),
        OrganizadorId = evento.OrganizadorId,
        CreadoEn = evento.CreadoEn,
        Asistentes = evento.Asistentes.Select(a => new AsistenteDto
        {
            Id = a.Id,
            NombreUsuario = a.NombreUsuario,
            Correo = a.Correo,
            RegistradoEn = a.RegistradoEn
        })
    };
}
