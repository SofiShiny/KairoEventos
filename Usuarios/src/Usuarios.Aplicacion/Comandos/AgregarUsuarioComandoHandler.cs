using MediatR;
using Microsoft.Extensions.Logging;
using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.Excepciones;
using Usuarios.Dominio.ObjetosValor;
using Usuarios.Dominio.Repositorios;
using Usuarios.Dominio.Servicios;

namespace Usuarios.Aplicacion.Comandos;

public class AgregarUsuarioComandoHandler : IRequestHandler<AgregarUsuarioComando, Guid>
{
    private readonly IRepositorioUsuarios _repositorio;
    private readonly IServicioKeycloak _servicioKeycloak;
    private readonly ILogger<AgregarUsuarioComandoHandler> _logger;

    public AgregarUsuarioComandoHandler(
        IRepositorioUsuarios repositorio,
        IServicioKeycloak servicioKeycloak,
        ILogger<AgregarUsuarioComandoHandler> logger)
    {
        _repositorio = repositorio;
        _servicioKeycloak = servicioKeycloak;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        AgregarUsuarioComando comando,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Agregando usuario: {Username}", comando.Username);

        // Validar unicidad de username
        if (await _repositorio.ExisteUsernameAsync(comando.Username, cancellationToken))
        {
            throw new UsernameDuplicadoException(comando.Username);
        }

        // Crear value objects
        var correo = Correo.Crear(comando.Correo);

        // Validar unicidad de correo
        if (await _repositorio.ExisteCorreoAsync(correo, cancellationToken))
        {
            throw new CorreoDuplicadoException(comando.Correo);
        }

        var telefono = Telefono.Crear(comando.Telefono);
        var direccion = Direccion.Crear(comando.Direccion);

        // Crear usuario
        var usuario = Usuario.Crear(
            comando.Username,
            comando.Nombre,
            correo,
            telefono,
            direccion,
            comando.Rol);

        try
        {
            // Crear en Keycloak primero y obtener el ID real
            var keycloakIdStr = await _servicioKeycloak.CrearUsuarioAsync(usuario, comando.Password, cancellationToken);
            var keycloakId = Guid.Parse(keycloakIdStr);

            // Re-crear el objeto usuario con el ID de Keycloak
            usuario = Usuario.Crear(
                comando.Username,
                comando.Nombre,
                correo,
                telefono,
                direccion,
                comando.Rol,
                keycloakId);

            // Luego persistir en BD
            await _repositorio.AgregarAsync(usuario, cancellationToken);

            _logger.LogInformation("Usuario agregado exitosamente: {UsuarioId}", usuario.Id);

            return usuario.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar usuario: {Username}", comando.Username);
            throw;
        }
    }
}
