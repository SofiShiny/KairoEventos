using MediatR;
using Microsoft.AspNetCore.Mvc;
using Eventos.Aplicacion.DTOs;
using Swashbuckle.AspNetCore.Filters;
using Eventos.Aplicacion.Comandos;
using Eventos.Aplicacion.Queries;

namespace Eventos.API.Controladores;

// Habilita validación automática de modelos y binding de parámetros 
//(rellenar parametros con datos de distintas fuentes)
[ApiController]
//[controller] Genera la ruta automáticamente basándose en el nombre de la clase
[Route("api/[controller]")]
public class EventosController : ControllerBase
{
    //IMediator: Patrón Mediator desacopla el controlador de los handlers
    // permitiendo cambiar la lógica sin modificar el controlador
    private readonly IMediator _mediator;
    private readonly ILogger<EventosController> _logger;

    public EventosController(IMediator mediator, ILogger<EventosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Queries (solo lectura) 
    // El mediator se encarga de entregar la query al handler correspondiente
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var resultado = await _mediator.Send(new ObtenerEventosQuery());
        //El patrón Result evita excepciones para flujo de control
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

    [HttpGet("{id}/disponible")]
    public async Task<IActionResult> VerificarDisponibilidad(Guid id)
    {
        var resultado = await _mediator.Send(new ObtenerEventoPorIdQuery(id));
        if (resultado.EsFallido || resultado.Valor == null) return NotFound();
        
        if (resultado.Valor.Estado == "Publicado") 
        {
            return Ok();
        }
        
        return BadRequest("El evento no está disponible para la venta");
    }

    // Comandos (escritura)
    // Los commands modifican estado, encapsula la intencion de crear un evento y sus datos
   
    [HttpPost]
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

            //El comando no ejecuta la logica sino que envia el comando al mediador
    //El mediador se encarga de entregar el comando al handler correspondiente con la logica de negocio 
            // Obtener el ID del usuario autenticado desde el token
            var organizadorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                             ?? User.FindFirst("sub")?.Value 
                             ?? "organizador-001"; // Fallback solo para desarrollo

            var comando = new CrearEventoComando(
                dto.Titulo,
                dto.Descripcion ?? string.Empty,
                dto.Ubicacion,
                dto.FechaInicio,
                dto.FechaFin,
                dto.MaximoAsistentes,
                organizadorId,
                dto.Categoria,
                dto.PrecioBase);

            var resultado = await _mediator.Send(comando);
            if (resultado.EsFallido) return BadRequest(resultado.Error);
            // CreatedAtAction Devuelve 201 Created con la URL del recurso creado
            return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor!.Id }, resultado.Valor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando evento");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] EventoUpdateDto dto)
    {
        try
        {
            // Obtener el evento actual para verificar permisos
            var eventoQuery = await _mediator.Send(new ObtenerEventoPorIdQuery(id));
            if (eventoQuery.EsFallido) return NotFound(eventoQuery.Error);

            // Verificar autorización
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                      ?? User.FindFirst("sub")?.Value;
            var isAdmin = User.IsInRole("admin");
            
            if (!isAdmin && eventoQuery.Valor?.OrganizadorId != userId)
            {
                return Forbid("No tienes permisos para editar este evento");
            }

            var comando = new ActualizarEventoComando(
                id,
                dto.Titulo,
                dto.Descripcion,
                dto.Ubicacion,
                dto.FechaInicio,
                dto.FechaFin,
                dto.MaximoAsistentes,
                dto.PrecioBase);

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

    [HttpPatch("{id}/publicar")]
    public async Task<IActionResult> Publicar(Guid id)
    {
        try
        {
            // Obtener el evento actual para verificar permisos
            var eventoQuery = await _mediator.Send(new ObtenerEventoPorIdQuery(id));
            if (eventoQuery.EsFallido) return NotFound(eventoQuery.Error);

            // Verificar autorización
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                      ?? User.FindFirst("sub")?.Value;
            var isAdmin = User.IsInRole("admin");
            
            if (!isAdmin && eventoQuery.Valor?.OrganizadorId != userId)
            {
                return Forbid("No tienes permisos para publicar este evento");
            }

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

    [HttpPatch("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        try
        {
            var resultado = await _mediator.Send(new CancelarEventoComando(id));
            if (resultado.EsFallido) return BadRequest(resultado.Error);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelando evento {Id}", id);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPatch("{id}/finalizar")]
    public async Task<IActionResult> Finalizar(Guid id)
    {
        try
        {
            // Obtener el evento actual para verificar permisos
            var eventoQuery = await _mediator.Send(new ObtenerEventoPorIdQuery(id));
            if (eventoQuery.EsFallido) return NotFound(eventoQuery.Error);

            // Verificar autorización
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                      ?? User.FindFirst("sub")?.Value;
            var isAdmin = User.IsInRole("admin");
            
            if (!isAdmin && eventoQuery.Valor?.OrganizadorId != userId)
            {
                return Forbid("No tienes permisos para finalizar este evento");
            }

            var resultado = await _mediator.Send(new FinalizarEventoComando(id));
            if (resultado.EsFallido) return BadRequest(resultado.Error);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizando evento {Id}", id);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("{id}/asistentes")]
    public async Task<IActionResult> RegistrarAsistente(Guid id, [FromBody] RegistrarAsistenteDto dto)
    {
        try
        {
            _logger.LogInformation("Iniciando registro de asistente en evento {EventoId}", id);
            
            if (dto == null) return BadRequest("Body requerido");
            
            var comando = new RegistrarAsistenteComando(id, dto.UsuarioId, dto.Nombre, dto.Correo);
            var resultado = await _mediator.Send(comando);
            
            if (resultado.EsFallido)
            {
                _logger.LogWarning("Fallo al registrar asistente en evento {EventoId}: {Error}", id, resultado.Error);
                return BadRequest(resultado.Error);
            }
            
            _logger.LogInformation("Asistente registrado exitosamente, recuperando evento actualizado...");
            
            // Recupera el evento actualizado para retornar el asistente recién agregado
            var eventoRes = await _mediator.Send(new ObtenerEventoPorIdQuery(id));
            
            if (eventoRes.EsFallido)
            {
                _logger.LogError("Error al recuperar evento {EventoId} después de registrar asistente: {Error}", id, eventoRes.Error);
                return NotFound(eventoRes.Error);
            }
            
            if (eventoRes.Valor == null || eventoRes.Valor.Asistentes == null || !eventoRes.Valor.Asistentes.Any())
            {
                _logger.LogError("Evento {EventoId} recuperado pero sin asistentes", id);
                return Problem("Error al recuperar el asistente registrado");
            }
            
            var asistente = eventoRes.Valor.Asistentes.Last();
            _logger.LogInformation("Asistente recuperado exitosamente: {AsistenteId}", asistente.Id);
            
            return Ok(asistente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado registrando asistente en evento {Id}", id);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id}")]
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

    [HttpPost("{id}/imagen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubirImagen(Guid id, IFormFile archivo)
    {
        try
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo no proporcionado o vacío.");

            using var stream = archivo.OpenReadStream();
            var comando = new ActualizarImagenEventoComando(id, stream, archivo.FileName);

            var resultado = await _mediator.Send(comando);
            
            if (resultado.EsFallido)
            {
                if (resultado.Error.Contains("No se encontró"))
                    return NotFound(resultado.Error);
                return BadRequest(resultado.Error);
            }

            return Ok(new { Url = resultado.Valor });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subiendo imagen para evento {Id}", id);
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Obtiene eventos sugeridos basados en categoría y fecha
    /// </summary>
    /// <param name="categoria">Categoría de interés</param>
    /// <param name="fechaDesde">Fecha mínima de inicio</param>
    /// <param name="top">Cantidad máxima de eventos a retornar</param>
    [HttpGet("sugeridos")]
    [ProducesResponseType(typeof(IEnumerable<EventoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventoDto>>> GetSugeridos(
        [FromQuery] string? categoria, 
        [FromQuery] DateTime? fechaDesde, 
        [FromQuery] int top = 3)
    {
        var query = new ObtenerEventosSugeridosQuery(categoria, fechaDesde, top);
        var resultado = await _mediator.Send(query);
        
        return Ok(resultado.Valor);
    }
}
