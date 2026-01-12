using Asientos.Aplicacion.Comandos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Asientos.API.Controllers;

[ApiController]
[Route("api/asientos/categorias")]
public class CategoriasAsientosController : ControllerBase
{
 private readonly IMediator _mediator;
 public CategoriasAsientosController(IMediator mediator) => _mediator = mediator;

 public record CategoriaCreateDto(
  [Required]
  [SwaggerSchema(Description="Id del mapa de asientos")]
  Guid MapaId,
  [Required, StringLength(40, MinimumLength=2)]
  [SwaggerSchema(Description="Nombre de la categoria")]
  string Nombre,
  [Range(0,double.MaxValue)]
  [SwaggerSchema(Description="Precio base opcional")]
  decimal? PrecioBase,
  [SwaggerSchema(Description="Indica si tiene prioridad especial")]
  bool TienePrioridad);

 [HttpPost]
 [SwaggerOperation(Summary="Agregar categoria", Description="Registra una categoria de asiento (ej: VIP, GENERAL) asociada al mapa.")]
 public async Task<IActionResult> Crear([FromBody] CategoriaCreateDto dto)
 {
  var catId = await _mediator.Send(new AgregarCategoriaComando(dto.MapaId, dto.Nombre, dto.PrecioBase, dto.TienePrioridad));
  return Ok(new { categoriaId = catId, nombre = dto.Nombre, precioBase = dto.PrecioBase, tienePrioridad = dto.TienePrioridad });
 }
}
