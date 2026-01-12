using Asientos.Aplicacion.Comandos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System;
using System.Threading.Tasks;

namespace Asientos.API.Controllers;

[ApiController]
[Route("api/asientos")]
public class AsientosController : ControllerBase
{
  private readonly IMediator _mediator;
  private readonly Asientos.Dominio.Repositorios.IRepositorioMapaAsientos _repo;

  public AsientosController(IMediator mediator, Asientos.Dominio.Repositorios.IRepositorioMapaAsientos repo)
  {
      _mediator = mediator;
      _repo = repo;
  }

  public record AsientoCreateDto(
      [Required]
      [SwaggerSchema(Description="Id del mapa de asientos")]
      Guid MapaId,
      [Range(1,int.MaxValue)]
      int Fila,
      [Range(1,int.MaxValue)]
      int Numero,
      [Required]
      string Categoria);

  public record ReservarAsientoDto(
      [Required]
      Guid MapaId,
      [Required]
      Guid AsientoId,
      [Required]
      Guid UsuarioId);

  public record LiberarAsientoDto(
      [Required]
      Guid MapaId,
      [Required]
      Guid AsientoId);

  [HttpGet("{id}")]
  [SwaggerOperation(Summary="Obtener info de un asiento")]
  public async Task<IActionResult> ObtenerPorId(Guid id)
  {
      var asiento = await _repo.ObtenerAsientoPorIdAsync(id, default);
      
      if (asiento == null) return NotFound();

      return Ok(new {
          Id = asiento.Id,
          MapaId = asiento.MapaId,
          Nombre = $"Asiento {asiento.Numero}",
          Precio = asiento.Categoria.PrecioBase ?? 0,
          Seccion = asiento.Categoria.Nombre, 
          Fila = asiento.Fila,
          Numero = asiento.Numero,
          EstaDisponible = !asiento.Reservado,
          UsuarioId = asiento.UsuarioId
      });
  }

  [HttpPost]
  [SwaggerOperation(Summary="Agregar asiento al mapa")]
  public async Task<IActionResult> Crear([FromBody] AsientoCreateDto dto)
  {
      var asientoId = await _mediator.Send(new AgregarAsientoComando(dto.MapaId, dto.Fila, dto.Numero, dto.Categoria));
      return Ok(new { asientoId });
  }

  [HttpPost("reservar")]
  [SwaggerOperation(Summary="Reservar/Bloquear asiento temporalmente", Description="Asigna un usuario a un asiento si está libre.")]
  public async Task<IActionResult> Reservar([FromBody] ReservarAsientoDto dto)
  {
      await _mediator.Send(new ReservarAsientoComando(dto.MapaId, dto.AsientoId, dto.UsuarioId));
      return Ok(new { message = "Asiento bloqueado exitosamente" });
  }

  [HttpPost("liberar")]
  [SwaggerOperation(Summary="Liberar asiento", Description="Pone el asiento como disponible y quita el usuario.")]
  public async Task<IActionResult> Liberar([FromBody] LiberarAsientoDto dto)
  {
      await _mediator.Send(new LiberarAsientoComando(dto.MapaId, dto.AsientoId));
      return Ok(new { message = "Asiento liberado" });
  }
}
