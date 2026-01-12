using Microsoft.AspNetCore.Mvc;
using MediatR;
using Servicios.Aplicacion.Comandos;
using Servicios.Dominio.Repositorios;
using Servicios.Dominio.Entidades;

namespace Servicios.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
            return Unauthorized(new { message = ex.Message });
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
    public async Task<ActionResult<IEnumerable<ReservaServicio>>> GetMisReservas(Guid usuarioId)
    {
        var reservas = await _repositorio.ObtenerReservasPorUsuarioAsync(usuarioId);
        return Ok(reservas);
    }
}
