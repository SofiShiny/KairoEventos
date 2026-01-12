using MediatR;
using Microsoft.Extensions.Logging;
using Usuarios.Dominio.Excepciones;
using Usuarios.Dominio.Repositorios;
using Usuarios.Dominio.Servicios;

namespace Usuarios.Aplicacion.Comandos;

public class EliminarUsuarioComandoHandler : IRequestHandler<EliminarUsuarioComando, Unit>
{
    private readonly IRepositorioUsuarios _repositorio;
    private readonly IServicioKeycloak _servicioKeycloak;
    private readonly ILogger<EliminarUsuarioComandoHandler> _logger;

    public EliminarUsuarioComandoHandler(
        IRepositorioUsuarios repositorio,
        IServicioKeycloak servicioKeycloak,
        ILogger<EliminarUsuarioComandoHandler> logger)
    {
        _repositorio = repositorio;
        _servicioKeycloak = servicioKeycloak;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        EliminarUsuarioComando comando,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Eliminando usuario: {UsuarioId}", comando.UsuarioId);

        // Obtener usuario existente
        var usuario = await _repositorio.ObtenerPorIdAsync(comando.UsuarioId, cancellationToken);
        
        if (usuario == null)
        {
            throw new UsuarioNoEncontradoException(comando.UsuarioId);
        }

        try
        {
            // Desactivar con método de negocio (eliminación lógica)
            usuario.Desactivar();

            // Desactivar en Keycloak
            await _servicioKeycloak.DesactivarUsuarioAsync(comando.UsuarioId, cancellationToken);

            // Persistir cambios
            await _repositorio.ActualizarAsync(usuario, cancellationToken);

            _logger.LogInformation("Usuario eliminado exitosamente: {UsuarioId}", comando.UsuarioId);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario: {UsuarioId}", comando.UsuarioId);
            throw;
        }
    }
}
