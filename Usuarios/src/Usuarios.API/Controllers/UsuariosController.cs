using MediatR;
using Microsoft.AspNetCore.Mvc;
using Usuarios.Aplicacion.Comandos;
using Usuarios.Aplicacion.Consultas;
using Usuarios.Aplicacion.DTOs;
using Usuarios.API.Filters;
using MassTransit;

namespace Usuarios.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsuariosController> _logger;

    private readonly IPublishEndpoint _publishEndpoint;

    public UsuariosController(
        IMediator mediator,
        ILogger<UsuariosController> logger,
        IPublishEndpoint publishEndpoint)
    {
        _mediator = mediator;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    /// <summary>
    /// Crea un nuevo usuario en el sistema
    /// </summary>
    /// <param name="dto">Datos del usuario a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>ID del usuario creado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Crear(
        [FromBody] CrearUsuarioDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creando usuario: {Username}", dto.Username);

        var comando = new AgregarUsuarioComando
        {
            Username = dto.Username,
            Nombre = dto.Nombre,
            Correo = dto.Correo,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            Rol = dto.Rol,
            Password = dto.Password
        };

        var usuarioId = await _mediator.Send(comando, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = usuarioId },
            usuarioId);
    }

    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Datos del usuario</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consultando usuario: {UsuarioId}", id);

        var query = new ConsultarUsuarioQuery { UsuarioId = id };
        var usuario = await _mediator.Send(query, cancellationToken);

        if (usuario == null)
        {
            _logger.LogWarning("Usuario no encontrado: {UsuarioId}", id);
            return NotFound();
        }

        return Ok(usuario);
    }

    /// <summary>
    /// Obtiene todos los usuarios activos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de usuarios activos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consultando todos los usuarios activos");

        var query = new ConsultarUsuariosQuery();
        var usuarios = await _mediator.Send(query, cancellationToken);

        return Ok(usuarios);
    }

    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    /// <param name="id">ID del usuario a actualizar</param>
    /// <param name="dto">Datos a actualizar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Sin contenido si la actualización fue exitosa</returns>
    [Auditoria]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> ActualizarPerfil(
        Guid id,
        [FromBody] ActualizarUsuarioDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Actualizando perfil de usuario: {UsuarioId}", id);

        var comando = new ActualizarUsuarioComando
        {
            UsuarioId = id,
            Nombre = dto.Nombre,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion
        };

        await _mediator.Send(comando, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Cambia la contraseña del usuario
    /// </summary>
    [Auditoria]
    [HttpPost("{id}/password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CambiarPassword(
        Guid id,
        [FromBody] CambiarPasswordDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cambiando password de usuario: {UsuarioId}", id);

        var comando = new CambiarPasswordComando
        {
            UsuarioId = id,
            PasswordActual = dto.PasswordActual,
            NuevoPassword = dto.NuevoPassword
        };

        var resultado = await _mediator.Send(comando, cancellationToken);
        
        if (!resultado) return BadRequest("No se pudo cambiar la contraseña");

        return Ok(new { mensaje = "Contraseña actualizada correctamente" });
    }

    /// <summary>
    /// Elimina (desactiva) un usuario
    /// </summary>
    /// <param name="id">ID del usuario a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Sin contenido si la eliminación fue exitosa</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Eliminar(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Eliminando usuario: {UsuarioId}", id);

        var comando = new EliminarUsuarioComando { UsuarioId = id };
        await _mediator.Send(comando, cancellationToken);

        return NoContent();
    }
}
