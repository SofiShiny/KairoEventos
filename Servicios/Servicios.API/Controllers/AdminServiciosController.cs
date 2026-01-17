using Microsoft.AspNetCore.Mvc;
using Servicios.Dominio.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Hangfire;

namespace Servicios.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "admin")] // TODO: Reactivar cuando Keycloak esté 100% afinado con roles
[AllowAnonymous] // Temporal para facilitar pruebas del usuario
public class AdminServiciosController : ControllerBase
{
    private readonly IProveedorExternoService _proveedorExterno;

    public AdminServiciosController(IProveedorExternoService proveedorExterno)
    {
        _proveedorExterno = proveedorExterno;
    }

    [HttpGet("externos")]
    public async Task<ActionResult<IEnumerable<ServicioExternoDto>>> GetServiciosExternos()
    {
        var servicios = await _proveedorExterno.ObtenerTodosLosServiciosAsync();
        return Ok(servicios);
    }

    [HttpPost("externos/update")]
    public async Task<IActionResult> UpdateServicioExterno(
        [FromBody] UpdateExternoRequest request, 
        [FromServices] IBackgroundJobClient backgroundJobs)
    {
        Console.WriteLine($"[ADMIN] Solicitud de actualización recibida para ID: {request.IdExterno}");
        try 
        {
            await _proveedorExterno.ActualizarServicioAsync(request.IdExterno, request.Precio, request.Disponible);
            Console.WriteLine($"[ADMIN] API Externa actualizada para ID: {request.IdExterno}");
            
            var todos = await _proveedorExterno.ObtenerTodosLosServiciosAsync();
            var item = todos.FirstOrDefault(s => s.IdServicioExterno == request.IdExterno);
            string nombre = item?.Nombre ?? "Servicio";

            backgroundJobs.Enqueue<Jobs.NotificationJob>(job => 
                job.NotificarCambioEstado(request.IdExterno, nombre, request.Precio, request.Disponible));
            
            Console.WriteLine($"[ADMIN] Job de Hangfire encolado para {nombre}");

            return Ok(new { message = "Servicio externo actualizado y notificación encolada" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ADMIN] ERROR: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public record UpdateExternoRequest(string IdExterno, decimal Precio, bool Disponible);
