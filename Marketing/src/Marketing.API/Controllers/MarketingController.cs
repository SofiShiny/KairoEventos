using Microsoft.AspNetCore.Mvc;
using Marketing.Aplicacion.CasosUso;
using Marketing.API.DTOs;
using Marketing.Aplicacion.Interfaces;

namespace Marketing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketingController : ControllerBase
{
    private readonly CrearCuponUseCase _crearUseCase;
    private readonly EnviarCuponUseCase _enviarUseCase;
    private readonly ValidarCuponUseCase _validarUseCase;
    private readonly IRepositorioCupones _repositorio;

    public MarketingController(
        CrearCuponUseCase crearUseCase,
        EnviarCuponUseCase enviarUseCase,
        ValidarCuponUseCase validarUseCase,
        IRepositorioCupones repositorio)
    {
        _crearUseCase = crearUseCase;
        _enviarUseCase = enviarUseCase;
        _validarUseCase = validarUseCase;
        _repositorio = repositorio;
    }

    [HttpPost("cupones")]
    public async Task<IActionResult> Crear([FromBody] CrearCuponDto dto)
    {
        try
        {
            var id = await _crearUseCase.EjecutarAsync(dto.Codigo, dto.Tipo, dto.Valor, dto.FechaExpiracion);
            return CreatedAtAction(nameof(Listar), new { id }, id);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("cupones")]
    public async Task<IActionResult> Listar()
    {
        var cupones = await _repositorio.ObtenerTodosAsync();
        var response = cupones.Select(c => new CuponResponseDto(
            c.Id, c.Codigo, c.TipoDescuento.ToString(), c.Valor, c.FechaExpiracion, 
            c.Estado.ToString(), c.UsuarioDestinatarioId, c.UsuarioQueLoUso, c.FechaUso));
        
        return Ok(response);
    }

    [HttpPost("cupones/{codigo}/enviar")]
    public async Task<IActionResult> Enviar(string codigo, [FromBody] EnviarCuponDto dto)
    {
        try
        {
            await _enviarUseCase.EjecutarAsync(codigo, dto.UsuarioId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("validar")]
    public async Task<IActionResult> Validar([FromBody] ValidarCuponDto dto)
    {
        var resultado = await _validarUseCase.EjecutarAsync(dto.Codigo);
        if (!resultado.EsValido)
            return BadRequest(new { mensaje = resultado.Mensaje });

        return Ok(resultado);
    }
}
