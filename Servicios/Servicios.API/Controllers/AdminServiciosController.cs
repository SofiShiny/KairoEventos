using Microsoft.AspNetCore.Mvc;
using Servicios.Dominio.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Hangfire;
using Servicios.Dominio.Repositorios;
using Servicios.Dominio.Entidades;

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

    [HttpGet("globales")]
    public async Task<ActionResult<IEnumerable<ServicioGlobal>>> GetServiciosGlobales([FromServices] IRepositorioServicios repo)
    {
        var servicios = await repo.ObtenerCatalogoAsync();
        return Ok(servicios);
    }

    [HttpPost("globales/update")]
    public async Task<IActionResult> UpdateServicioGlobal(
        [FromBody] UpdateGlobalRequest request,
        [FromServices] IRepositorioServicios repo)
    {
        var servicio = await repo.ObtenerServicioPorIdAsync(request.Id);
        if (servicio == null) return NotFound();

        // Actualizamos nombre y precio
        // Como ServicioGlobal tiene un constructor y props, usamos el repositorio para actualizar
        // o modificamos la entidad si el seguimiento de cambios está activo.
        
        // Asumiendo que podemos actualizar las propiedades (necesitamos verificar si tienen setters)
        servicio.Update(request.Nombre, request.Precio);
        
        await repo.ActualizarServicioAsync(servicio);
        await repo.SaveAsync();

        return Ok(new { message = "Servicio global actualizado correctamente" });
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
public record UpdateGlobalRequest(Guid Id, string Nombre, decimal Precio);
