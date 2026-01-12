using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Usuarios.Application.Dtos;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;
using Usuarios.Core.Services;

namespace Usuarios.Infrastructure.Keycloak
{
    public class AccessManagementKeycloak : IAccesManagement<UsuarioKeycloak>
    {
        private string? _token;
        private readonly IWebRequest _webRequest;
        private readonly ILogger<AccessManagementKeycloak> _logger;
        public AccessManagementKeycloak(IWebRequest webRequest, ILogger<AccessManagementKeycloak> logger)
        {
            _webRequest = webRequest;
            _logger = logger;
        }

        /// <summary>
        /// Se necesita para setear el token admin para realizar las peticiones
        /// <list type="number">
        /// <item><param name="token"> El nuevo <em>token admin</em>.</param></item>
        /// </list>
        /// </summary>
        public void SetToken(string token)
        {
            _logger.LogInformation("Actualizamos el token de admin de keycloak");
            _token = token;
        }

        /// <summary>
        /// Se necesita para realizar acciones a un usuario en keycloak
        /// <list type="number">
        /// <item><param name="correoUsuario">El <em>correo</em> del usuario</param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Id</strong> del usuario.</returns>
        private async Task<string?> ObtenerIdUsuario(string correoUsuario)
        {
            var usuario = await _webRequest.GetAsync(
                $"http://localhost:8080/admin/realms/myrealm/users?email={correoUsuario}",
                new Dictionary<string, string> { { "Authorization", $"Bearer {_token}" } });

            _logger.LogInformation(
                "Por medio de la peticion a keycloak se obtuvo el usuario y despues traemos el id del mismo");

            return JsonDocument.Parse(usuario).RootElement[0].GetProperty("id").GetString();
        }

        /// <summary>
        /// Se necesita para asignar un rol a un usuario.
        /// <list type="number">
        /// <item><param name="nombreRol">El <em>nombre</em> del rol</param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Id</strong> del rol.</returns>
        private async Task<string?> ObtenerRol(string nombreRol)
        {
            var rol = await _webRequest.GetAsync($"http://localhost:8080/admin/realms/myrealm/roles/{nombreRol}",
                new Dictionary<string, string> { { "Authorization", $"Bearer {_token}" } });

            _logger.LogInformation(
                "Por medio de una peticion a keycloak se obtuvo el rol y despues traemos el id del mismo");

            return JsonDocument.Parse(rol).RootElement.GetProperty("id").GetString();
        }

        /// <summary>
        /// Al agregar un usuario en keycloak se necesita asignar el rol designado por el sistema
        /// <list type="number">
        /// <item><param name="correoUsuario">El <em>correo</em> del usuario</param></item>
        /// <item><param name="nombreRol">El <em>nombre</em> del rol</param></item>
        /// </list>
        /// </summary>
        public async Task AsignarRol(string correoUsuario, string nombreRol)
        {
            var id = await this.ObtenerIdUsuario(correoUsuario);
            var rolId = await this.ObtenerRol(nombreRol);

            var rolAsignar = new[]
            {
                new
                {
                    id = rolId,
                    name = nombreRol
                }
            };

            _logger.LogInformation("Hacemos una peticion a keycloak para asignar el rol {idRol} al usuario {id}", rolId,
                id);

            await _webRequest.PostAsync(
                $"http://localhost:8080/admin/realms/myrealm/users/{id}/role-mappings/realm",
                new StringContent(JsonSerializer.Serialize(rolAsignar), Encoding.UTF8, "application/json"),
                new Dictionary<string, string> { { "Authorization", $"Bearer {_token}" } });
        }

        /// <summary>
        /// Se necesita para que el usuario pueda entrar en la plataforma, ya que se usuara keycloak para iniciar sesion.
        /// <list type="number">
        /// <item><param name="usuario">El <em>usuario</em> en el formato de keycloak</param></item>
        /// <item><param name="rol">El <em>nombre</em> del rol</param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Id</strong> del usuario registrado.</returns>
        public async Task<Guid> AgregarUsuario(UsuarioKeycloak usuario, string rol)
        {
            var usuarioEncoding =
                new StringContent(JsonSerializer.Serialize(usuario), Encoding.UTF8, "application/json");

            _logger.LogInformation("Hacemos una peticion a keycloak para agregar el usuario {correo} a keycloak",
                usuario.Email);

            await _webRequest.PostAsync("http://localhost:8080/admin/realms/myrealm/users", usuarioEncoding,
                new Dictionary<string, string> { { "Authorization", $"Bearer {_token}" } });

            await this.AsignarRol(usuario.Email, rol);

            _logger.LogInformation("Rol asignado");

            var id = await this.ObtenerIdUsuario(usuario.Email);
            return Guid.Parse(id!);
        }

        /// <summary>
        /// Se necesita para modificar los datos del usuario y asi alla concordancia entre la base de datos y keycloak, y asi tener un backup de la base de datos actualizada
        /// <list type="number">
        /// <item><param name="usuario">El <em>usuario</em> en el formato de keycloak</param></item>
        /// <item><param name="id">El <em>id</em> del usuario</param></item>
        /// </list>
        /// </summary>
        public async Task ModificarUsuario(UsuarioKeycloak usuario, Guid id)
        {
            _logger.LogInformation("Hacemos peticion a keycloak para eliminar el usuario");

            await _webRequest.PutAsync($"http://localhost:8080/admin/realms/myrealm/users/{id}",
                new StringContent(JsonSerializer.Serialize(usuario), Encoding.UTF8, "application/json"),
                new Dictionary<string, string> { { "Authorization", $"Bearer {_token}" } });
        }
        /// <summary>
        /// Se necesita para eliminar un usuario de keycloak, por que si no podria entrar otra vez a la plataforma.
        /// <list type="number">
        /// <item><param name="id">El <em>id</em> del usuario</param></item>
        /// </list>
        /// </summary>
        public async Task EliminarUsuario(Guid id)
        {
            _logger.LogInformation("Hacemos peticion a keycloak para eliminar el usuario");

            await _webRequest.DeleteAsync($"http://localhost:8080/admin/realms/myrealm/users/{id}",
                new Dictionary<string, string> { { "Authorization", $"Bearer {_token}" } });
        }
        /// <summary>
        /// Se necesita para verificar si el usuario esta registrado en keycloak y si se necesita consultar en keycloak para despues insertarlo en base de datos
        /// <list type="number">
        /// <item><param name="id">El <em>id</em> del usuario</param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Usuario</strong> que se consulto.</returns>
        public async Task<UsuarioKeycloak> ConsultarUsuarioPorId(string id)
        {
            _logger.LogInformation("Hacemos una peticion a keycloak para consultar el usuario");

            var usuario = await _webRequest.GetAsync($"http://localhost:8080/admin/realms/myrealm/users/{id}",
                new Dictionary<string, string> { { "Authorization", $"Bearer {_token}" } });
            return JsonSerializer.Deserialize<UsuarioKeycloak>(usuario);
        }
    }
}
