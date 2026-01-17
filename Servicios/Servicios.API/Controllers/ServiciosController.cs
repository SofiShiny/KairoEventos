using Microsoft.AspNetCore.Mvc;
using MediatR;
using Servicios.Aplicacion.Comandos;
using Servicios.Dominio.Repositorios;
using Servicios.Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;

namespace Servicios.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Forzamos acceso público para descartar Auth Middleware
public class ServiciosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRepositorioServicios _repositorio;

    public ServiciosController(IMediator mediator, IRepositorioServicios repositorio)
    {
        _mediator = mediator;
        _repositorio = repositorio;
    }

    [HttpGet("catalogo")]
    public async Task<ActionResult<IEnumerable<ServicioGlobal>>> GetCatalogo()
    {
        var catalogo = await _repositorio.ObtenerCatalogoAsync();
        return Ok(catalogo);
    }

    [HttpPost("reservar")]
    public async Task<ActionResult<Guid>> Reservar([FromBody] ReservarServicioCommand command)
    {
        try 
        {
            var id = await _mediator.Send(command);
            return StatusCode(201, id);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Retornamos 403 para diferenciarlo de un error de Autenticación (401 - Token inválido)
            return StatusCode(403, new { message = ex.Message, error = "No tiene entrada para este evento" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno al procesar la reserva", detail = ex.Message });
        }
    }

    [HttpGet("mis-reservas/{usuarioId:guid}")]
    public async Task<ActionResult> GetMisReservas(Guid usuarioId)
    {
        var reservas = await _repositorio.ObtenerReservasPorUsuarioAsync(usuarioId);
        
        // Proyección explícita para asegurar que OrdenEntradaId se serialice
        // (System.Text.Json a veces ignora propiedades con private set por defecto)
        var resultado = reservas.Select(r => new 
        {
            r.Id,
            r.UsuarioId,
            r.EventoId,
            r.ServicioGlobalId,
            r.OrdenEntradaId,
            r.Estado,
            r.FechaCreacion
        });

        return Ok(resultado);
    }
}
