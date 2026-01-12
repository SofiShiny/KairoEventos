using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Reportes.Dominio.ModelosLectura;
using Reportes.Infraestructura.Persistencia;
using System.Security.Claims;

namespace Reportes.API.Controladores
{
    [ApiController]
    [Route("api/reportes/historial")]
    public class HistorialController : ControllerBase
    {
        private readonly ReportesMongoDbContext _context;
        private readonly ILogger<HistorialController> _logger;

        public HistorialController(ReportesMongoDbContext context, ILogger<HistorialController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("mio")]
        public async Task<IActionResult> ObtenerHistorialMio()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim))
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(usuarioIdClaim, out Guid usuarioId))
            {
                return BadRequest("UsuarioId inválido en el token");
            }

            _logger.LogInformation("Consultando historial para usuario: {UsuarioId}", usuarioId);

            var filter = Builders<ElementoHistorial>.Filter.Eq(t => t.UsuarioId, usuarioId);
            var sort = Builders<ElementoHistorial>.Sort.Descending(t => t.Fecha);

            var historial = await _context.Timeline
                .Find(filter)
                .Sort(sort)
                .ToListAsync();

            return Ok(historial);
        }
    }
}
