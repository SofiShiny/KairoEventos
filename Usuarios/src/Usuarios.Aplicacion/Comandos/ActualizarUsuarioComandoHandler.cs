using MediatR;
using Microsoft.Extensions.Logging;
using Usuarios.Dominio.Excepciones;
using Usuarios.Dominio.ObjetosValor;
using Usuarios.Dominio.Repositorios;
using Usuarios.Dominio.Servicios;

namespace Usuarios.Aplicacion.Comandos;

public class ActualizarUsuarioComandoHandler : IRequestHandler<ActualizarUsuarioComando, Unit>
{
    private readonly IRepositorioUsuarios _repositorio;
    private readonly IServicioKeycloak _servicioKeycloak;
    private readonly ILogger<ActualizarUsuarioComandoHandler> _logger;

    public ActualizarUsuarioComandoHandler(
        IRepositorioUsuarios repositorio,
        IServicioKeycloak servicioKeycloak,
        ILogger<ActualizarUsuarioComandoHandler> logger)
    {
        _repositorio = repositorio;
        _servicioKeycloak = servicioKeycloak;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        ActualizarUsuarioComando comando,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Actualizando usuario: {UsuarioId}", comando.UsuarioId);

        // Obtener usuario existente
        var usuario = await _repositorio.ObtenerPorIdAsync(comando.UsuarioId, cancellationToken);
        
        if (usuario == null)
        {
            throw new UsuarioNoEncontradoException(comando.UsuarioId);
        }

        // Crear value objects
        var telefono = Telefono.Crear(comando.Telefono);
        var direccion = Direccion.Crear(comando.Direccion);

        // Actualizar con método de negocio
        usuario.Actualizar(comando.Nombre, telefono, direccion);

        try
        {
            // Actualizar en Keycloak
            await _servicioKeycloak.ActualizarUsuarioAsync(usuario, cancellationToken);

            // Persistir cambios
            await _repositorio.ActualizarAsync(usuario, cancellationToken);

            _logger.LogInformation("Usuario actualizado exitosamente: {UsuarioId}", comando.UsuarioId);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario: {UsuarioId}", comando.UsuarioId);
            throw;
        }
    }
}
