using Asientos.Aplicacion.Comandos;
using Asientos.Aplicacion.Queries;
using Asientos.Infraestructura.Persistencia;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Asientos.API.Controllers;

[ApiController]
[Route("api/asientos/mapas")]
public class MapasAsientosController : ControllerBase
{
 private readonly IMediator _mediator;
 private readonly AsientosDbContext _context;
 
 public MapasAsientosController(IMediator mediator, AsientosDbContext context)
 {
   _mediator = mediator;
   _context = context;
 }

 public record CrearMapaRequest([SwaggerSchema(Description="Id del evento publicado")] Guid EventoId);

 [HttpPost]
 [SwaggerOperation(Summary="Crear mapa de asientos", Description="Inicializa un mapa vacio para un evento publicado")]
 public async Task<IActionResult> Crear([FromBody] CrearMapaRequest body)
 {
  var mapaId = await _mediator.Send(new CrearMapaAsientosComando(body.EventoId));
  return CreatedAtAction(nameof(Obtener), new { id = mapaId }, new { mapaId });
 }

 [HttpGet("{id}")]
 [SwaggerOperation(Summary="Obtener mapa", Description="Devuelve categorias y asientos actuales del mapa")]
 public async Task<IActionResult> Obtener(Guid id)
 {
  var mapa = await _mediator.Send(new ObtenerMapaAsientosQuery(id));
  if (mapa == null) return NotFound();
  return Ok(mapa);
 }

 [HttpGet("evento/{eventoId}")]
 [SwaggerOperation(Summary="Obtener mapa por evento", Description="Devuelve el mapa de asientos de un evento específico")]
 public async Task<IActionResult> ObtenerPorEvento(Guid eventoId)
 {
  var mapa = await _context.Mapas
    .Include(m => m.Asientos)
      .ThenInclude(a => a.Categoria)
    .FirstOrDefaultAsync(m => m.EventoId == eventoId);
  
  if (mapa == null) 
    return NotFound(new { message = "No se encontró un mapa para este evento" });
  
  return Ok(new { 
    id = mapa.Id,
    eventoId = mapa.EventoId,
    asientos = mapa.Asientos.Select(a => new {
      id = a.Id,
      fila = a.Fila,
      numero = a.Numero,
      categoria = a.Categoria.Nombre,
      precio = a.Categoria.PrecioBase ?? 0,
      reservado = a.Reservado,
      usuarioId = a.UsuarioId
    }).ToList()
  });
 }
}
