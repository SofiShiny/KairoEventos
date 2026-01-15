using MediatR;
using Microsoft.Extensions.Logging;
using Usuarios.Aplicacion.Comandos;
using Usuarios.Dominio.Repositorios;
using Usuarios.Dominio.ObjetosValor;

namespace Usuarios.Aplicacion.Handlers;

/// <summary>
/// Handler para actualizar el perfil del usuario
/// </summary>
public class ActualizarPerfilComandoHandler : IRequestHandler<ActualizarPerfilComando, bool>
{
    private readonly IRepositorioUsuarios _usuarioRepositorio;
    private readonly ILogger<ActualizarPerfilComandoHandler> _logger;

    public ActualizarPerfilComandoHandler(
        IRepositorioUsuarios usuarioRepositorio,
        ILogger<ActualizarPerfilComandoHandler> logger)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _logger = logger;
    }

    public async Task<bool> Handle(ActualizarPerfilComando request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Actualizando perfil del usuario {UsuarioId}", request.UsuarioId);

            // Obtener el usuario
            var usuario = await _usuarioRepositorio.ObtenerPorIdAsync(request.UsuarioId);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario {UsuarioId} no encontrado", request.UsuarioId);
                return false;
            }

            // Crear objetos de valor
            var telefono = Telefono.Crear(request.Telefono);
            var direccion = Direccion.Crear(request.Direccion);

            // Actualizar el usuario usando el método existente
            usuario.Actualizar(request.Nombre, telefono, direccion);

            // Guardar cambios
            await _usuarioRepositorio.ActualizarAsync(usuario);

            _logger.LogInformation("Perfil del usuario {UsuarioId} actualizado exitosamente", request.UsuarioId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el perfil del usuario {UsuarioId}", request.UsuarioId);
            throw;
        }
    }
}
