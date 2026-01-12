using Microsoft.AspNetCore.Mvc;
using Pagos.Aplicacion.DTOs;
using Pagos.Dominio.Entidades;
using Pagos.Dominio.Interfaces;
using Pagos.Aplicacion.CasosUso;
using Hangfire;

namespace Pagos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagosController : ControllerBase
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRepositorioTransacciones _repositorio;
    private readonly ILogger<PagosController> _logger;

    public PagosController(
        IBackgroundJobClient backgroundJobClient,
        IRepositorioTransacciones repositorio,
        ILogger<PagosController> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _repositorio = repositorio;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Procesar([FromBody] CrearPagoDto dto)
    {
        _logger.LogInformation("Procesando pago para Orden: {OrdenId}, Usuario: {UsuarioId}, Monto: {Monto}", 
            dto.OrdenId, dto.UsuarioId, dto.Monto);

        if (string.IsNullOrEmpty(dto.Tarjeta) || dto.Tarjeta.Length < 4)
        {
            _logger.LogWarning("Número de tarjeta inválido o muy corto");
            return BadRequest(new { mensaje = "Número de tarjeta inválido" });
        }

        try 
        {
            var transaccion = new Transaccion
            {
                Id = Guid.NewGuid(),
                OrdenId = dto.OrdenId,
                UsuarioId = dto.UsuarioId,
                Monto = dto.Monto,
                TarjetaMascara = $"****-****-****-{dto.Tarjeta[^4..]}",
                Estado = Pagos.Dominio.Modelos.EstadoTransaccion.Procesando,
                FechaCreacion = DateTime.UtcNow
            };

            await _repositorio.AgregarAsync(transaccion);

            // Encolar trabajo asíncrono en Hangfire
            _backgroundJobClient.Enqueue<IProcesarPagoUseCase>(x => 
                x.EjecutarAsync(transaccion.Id, dto.Tarjeta));

            return Accepted(new { transaccionId = transaccion.Id, estado = "Procesando" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar el pago");
            return StatusCode(500, new { mensaje = "Error interno al procesar el pago", detalle = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var transacciones = await _repositorio.ObtenerTodasAsync();
        return Ok(transacciones);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(Guid id)
    {
        var tx = await _repositorio.ObtenerPorIdAsync(id);
        return tx != null ? Ok(tx) : NotFound();
    }
}
