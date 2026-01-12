using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pagos.Aplicacion.Servicios;

namespace Pagos.API.Controllers;

[ApiController]
[Route("api/pagos/cupones")]
public class CuponesController : ControllerBase
{
    private readonly ICuponServicio _cuponServicio;
    private readonly ILogger<CuponesController> _logger;

    public CuponesController(ICuponServicio cuponServicio, ILogger<CuponesController> logger)
    {
        _cuponServicio = cuponServicio;
        _logger = logger;
    }

    /// <summary>
    /// Valida un cupón y calcula el descuento
    /// </summary>
    [HttpPost("validar")]
    [AllowAnonymous]
    public async Task<ActionResult<ResultadoValidacionCupon>> ValidarCupon(
        [FromBody] ValidarCuponRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _cuponServicio.ValidarCuponAsync(
                request.Codigo,
                request.EventoId,
                request.MontoTotal,
                cancellationToken
            );

            if (!resultado.EsValido)
            {
                return BadRequest(new { mensaje = resultado.Mensaje });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar cupón {Codigo}", request.Codigo);
            return StatusCode(500, new { mensaje = "Error al validar el cupón" });
        }
    }

    /// <summary>
    /// Crea un cupón general (reutilizable)
    /// </summary>
    [HttpPost("general")]
    [Authorize(Roles = "admin,organizador")]
    public async Task<ActionResult<CuponDto>> CrearCuponGeneral(
        [FromBody] CrearCuponGeneralRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = new CrearCuponGeneralDto(
                request.Codigo,
                request.PorcentajeDescuento,
                request.EventoId,
                request.EsGlobal,
                request.FechaExpiracion,
                request.LimiteUsos
            );

            var cupon = await _cuponServicio.CrearCuponGeneralAsync(dto, cancellationToken);

            return CreatedAtAction(
                nameof(ObtenerCuponesPorEvento),
                new { eventoId = cupon.EventoId ?? Guid.Empty },
                new CuponDto
                {
                    Id = cupon.Id,
                    Codigo = cupon.Codigo,
                    PorcentajeDescuento = cupon.PorcentajeDescuento,
                    Tipo = cupon.Tipo.ToString(),
                    Estado = cupon.Estado.ToString(),
                    EventoId = cupon.EventoId,
                    FechaCreacion = cupon.FechaCreacion,
                    FechaExpiracion = cupon.FechaExpiracion
                }
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cupón general");
            return StatusCode(500, new { mensaje = "Error al crear el cupón" });
        }
    }

    /// <summary>
    /// Genera un lote de cupones únicos
    /// </summary>
    [HttpPost("lote")]
    [Authorize(Roles = "admin,organizador")]
    public async Task<ActionResult<List<CuponDto>>> GenerarLoteCupones(
        [FromBody] GenerarLoteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = new GenerarLoteCuponesDto(
                request.Cantidad,
                request.PorcentajeDescuento,
                request.EventoId,
                request.FechaExpiracion
            );

            var cupones = await _cuponServicio.GenerarLoteCuponesAsync(dto, cancellationToken);

            var cuponesDto = cupones.Select(c => new CuponDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
                PorcentajeDescuento = c.PorcentajeDescuento,
                Tipo = c.Tipo.ToString(),
                Estado = c.Estado.ToString(),
                EventoId = c.EventoId,
                FechaCreacion = c.FechaCreacion,
                FechaExpiracion = c.FechaExpiracion
            }).ToList();

            return Ok(cuponesDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar lote de cupones");
            return StatusCode(500, new { mensaje = "Error al generar los cupones" });
        }
    }

    /// <summary>
    /// Obtiene todos los cupones de un evento
    /// </summary>
    [HttpGet("evento/{eventoId}")]
    [Authorize(Roles = "admin,organizador")]
    public async Task<ActionResult<List<CuponDto>>> ObtenerCuponesPorEvento(
        Guid eventoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var cupones = await _cuponServicio.ObtenerCuponesPorEventoAsync(eventoId, cancellationToken);
            return Ok(cupones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cupones del evento {EventoId}", eventoId);
            return StatusCode(500, new { mensaje = "Error al obtener los cupones" });
        }
    }

    /// <summary>
    /// Obtiene todos los cupones globales
    /// </summary>
    [HttpGet("globales")]
    [Authorize(Roles = "admin,organizador")]
    public async Task<ActionResult<List<CuponDto>>> ObtenerCuponesGlobales(CancellationToken cancellationToken)
    {
        try
        {
            var cupones = await _cuponServicio.ObtenerCuponesGlobalesAsync(cancellationToken);
            return Ok(cupones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cupones globales");
            return StatusCode(500, new { mensaje = "Error al obtener los cupones" });
        }
    }
}

// Request DTOs
public record ValidarCuponRequest(string Codigo, Guid? EventoId, decimal MontoTotal);

public record CrearCuponGeneralRequest(
    string Codigo,
    decimal PorcentajeDescuento,
    Guid? EventoId,
    bool EsGlobal,
    DateTime? FechaExpiracion,
    int? LimiteUsos
);

public record GenerarLoteRequest(
    int Cantidad,
    decimal PorcentajeDescuento,
    Guid? EventoId,
    DateTime? FechaExpiracion
);
