using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.Enums;
using Usuarios.Dominio.Servicios;

namespace Usuarios.Infraestructura.Servicios
{
    public class ServicioKeycloak : IServicioKeycloak
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServicioKeycloak> _logger;
        private string? _adminToken;
        
        public ServicioKeycloak(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ServicioKeycloak> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }
        
        public async Task<string> CrearUsuarioAsync(
            Usuario usuario,
            string password,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creando usuario en Keycloak: {Username}", usuario.Username);
                
                await EnsureAdminTokenAsync(cancellationToken);
                
                var keycloakUser = new
                {
                    username = usuario.Username,
                    email = usuario.Correo.Valor,
                    firstName = usuario.Nombre,
                    enabled = usuario.EstaActivo,
                    credentials = new[]
                    {
                        new
                        {
                            type = "password",
                            value = password,
                            temporary = false
                        }
                    }
                };
                
                var adminUrl = _configuration["Keycloak:AdminUrl"] ?? "http://keycloak:8080/admin/realms/Kairo";
                var content = new StringContent(
                    JsonSerializer.Serialize(keycloakUser),
                    Encoding.UTF8,
                    "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{adminUrl}/users")
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Bearer {_adminToken}");
                
                var response = await _httpClient.SendAsync(request, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Error al crear usuario en Keycloak. Status: {Status}, Body: {Body}", response.StatusCode, errorBody);
                    var ex = new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).");
                    ex.Data["ResponseBody"] = errorBody;
                    throw ex;
                }
                
                // Extraer el ID de Keycloak (es el segmento después de /users/)
                var locationUrl = response.Headers.Location?.ToString() ?? string.Empty;
                var keycloakUserId = string.Empty;
                
                if (!string.IsNullOrEmpty(locationUrl))
                {
                    var usersSegment = "/users/";
                    var index = locationUrl.LastIndexOf(usersSegment);
                    if (index != -1)
                    {
                        keycloakUserId = locationUrl.Substring(index + usersSegment.Length).TrimEnd('/');
                    }
                }

                if (string.IsNullOrEmpty(keycloakUserId))
                {
                    _logger.LogWarning("No se pudo extraer el ID de Keycloak de la cabecera Location: {Location}. Usando ID local como fallback.", locationUrl);
                    keycloakUserId = usuario.Id.ToString();
                }
                else
                {
                    _logger.LogInformation("ID de Keycloak extraído: {KeycloakId}", keycloakUserId);
                }
                
                // Asignar rol usando el ID real de Keycloak
                await AsignarRolAsync(keycloakUserId, usuario.Rol, cancellationToken);
                
                _logger.LogInformation("Usuario creado exitosamente en Keycloak: {UsuarioId}", usuario.Id);
                
                return keycloakUserId;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error al crear usuario en Keycloak: {Username}", usuario.Username);
                throw new HttpRequestException($"Error al crear usuario en Keycloak: {ex.Message}", ex);
            }
        }
        
        public async Task ActualizarUsuarioAsync(
            Usuario usuario,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Actualizando usuario en Keycloak: {UsuarioId}", usuario.Id);
                
                await EnsureAdminTokenAsync(cancellationToken);
                
                var keycloakUser = new
                {
                    username = usuario.Username,
                    email = usuario.Correo.Valor,
                    firstName = usuario.Nombre,
                    enabled = usuario.EstaActivo
                };
                
                var adminUrl = _configuration["Keycloak:AdminUrl"] ?? "http://keycloak:8080/admin/realms/Kairo";
                var content = new StringContent(
                    JsonSerializer.Serialize(keycloakUser),
                    Encoding.UTF8,
                    "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Put, $"{adminUrl}/users/{usuario.Id}")
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Bearer {_adminToken}");
                
                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Usuario actualizado exitosamente en Keycloak: {UsuarioId}", usuario.Id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario en Keycloak: {UsuarioId}", usuario.Id);
                throw new HttpRequestException($"Error al actualizar usuario en Keycloak: {ex.Message}", ex);
            }
        }
        
        public async Task DesactivarUsuarioAsync(
            Guid usuarioId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Desactivando usuario en Keycloak: {UsuarioId}", usuarioId);
                
                await EnsureAdminTokenAsync(cancellationToken);
                
                var keycloakUser = new
                {
                    enabled = false
                };
                
                var adminUrl = _configuration["Keycloak:AdminUrl"] ?? "http://keycloak:8080/admin/realms/Kairo";
                var content = new StringContent(
                    JsonSerializer.Serialize(keycloakUser),
                    Encoding.UTF8,
                    "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Put, $"{adminUrl}/users/{usuarioId}")
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Bearer {_adminToken}");
                
                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Usuario desactivado exitosamente en Keycloak: {UsuarioId}", usuarioId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error al desactivar usuario en Keycloak: {UsuarioId}", usuarioId);
                throw new HttpRequestException($"Error al desactivar usuario en Keycloak: {ex.Message}", ex);
            }
        }
        
        public async Task AsignarRolAsync(
            string usuarioId,
            Rol rol,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Asignando rol {Rol} a usuario {UsuarioId} en Keycloak", rol, usuarioId);
                
                await EnsureAdminTokenAsync(cancellationToken);
                
                var roleName = rol switch
                {
                    Rol.User => "User",
                    Rol.Admin => "Admin",
                    Rol.Organizator => "Organizator",
                    _ => throw new ArgumentException($"Rol no vÃ¡lido: {rol}")
                };
                
                // Obtener el rol de Keycloak
                var adminUrl = _configuration["Keycloak:AdminUrl"] ?? "http://keycloak:8080/admin/realms/Kairo";
                var getRoleRequestUrl = $"{adminUrl}/roles/{roleName}";
                var getRoleRequest = new HttpRequestMessage(HttpMethod.Get, getRoleRequestUrl);
                getRoleRequest.Headers.Add("Authorization", $"Bearer {_adminToken}");
                
                var getRoleResponse = await _httpClient.SendAsync(getRoleRequest, cancellationToken);
                
                if (!getRoleResponse.IsSuccessStatusCode)
                {
                    var errorBody = await getRoleResponse.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Error al buscar rol '{RoleName}' en Keycloak. Status: {Status}, Url: {Url}, Body: {Body}", 
                        roleName, getRoleResponse.StatusCode, getRoleRequestUrl, errorBody);
                    var ex = new HttpRequestException($"Error al buscar rol en Keycloak: {(int)getRoleResponse.StatusCode}");
                    ex.Data["ResponseBody"] = errorBody;
                    throw ex;
                }
                
                var roleJson = await getRoleResponse.Content.ReadAsStringAsync(cancellationToken);
                var roleData = JsonSerializer.Deserialize<JsonElement>(roleJson);
                var roleId = roleData.GetProperty("id").GetString();
                
                // Asignar el rol al usuario
                var rolesToAssign = new[]
                {
                    new
                    {
                        id = roleId,
                        name = roleName
                    }
                };
                
                var content = new StringContent(
                    JsonSerializer.Serialize(rolesToAssign),
                    Encoding.UTF8,
                    "application/json");
                
                var assignRoleRequestUrl = $"{adminUrl}/users/{usuarioId}/role-mappings/realm";
                var assignRoleRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    assignRoleRequestUrl)
                {
                    Content = content
                };
                assignRoleRequest.Headers.Add("Authorization", $"Bearer {_adminToken}");
                
                var assignRoleResponse = await _httpClient.SendAsync(assignRoleRequest, cancellationToken);
                
                if (!assignRoleResponse.IsSuccessStatusCode)
                {
                    var errorBody = await assignRoleResponse.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Error al asignar rol en Keycloak. Status: {Status}, Url: {Url}, Body: {Body}", 
                        assignRoleResponse.StatusCode, assignRoleRequestUrl, errorBody);
                    var ex = new HttpRequestException($"Response status code does not indicate success: {(int)assignRoleResponse.StatusCode} ({assignRoleResponse.ReasonPhrase}).");
                    ex.Data["ResponseBody"] = errorBody;
                    throw ex;
                }
                
                _logger.LogInformation("Rol {Rol} asignado exitosamente a usuario {UsuarioId} en Keycloak", rol, usuarioId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error al asignar rol {Rol} a usuario {UsuarioId} en Keycloak", rol, usuarioId);
                throw new HttpRequestException($"Error al asignar rol en Keycloak: {ex.Message}", ex);
            }
        }
        
        private async Task EnsureAdminTokenAsync(CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(_adminToken))
            {
                return;
            }
            
            try
            {
                _logger.LogDebug("Obteniendo token de administrador de Keycloak");
                
                var authority = _configuration["Keycloak:Authority"] ?? "http://keycloak:8080/realms/Kairo";
                var clientId = _configuration["Keycloak:ClientId"] ?? "admin-cli";
                var clientSecret = _configuration["Keycloak:ClientSecret"] ?? "";
                
                // Para obtener el token de admin global, debemos ir al realm 'master'
                var masterAuthority = authority.Replace("/realms/Kairo", "/realms/master");
                var tokenEndpoint = $"{masterAuthority}/protocol/openid-connect/token";
                
                Dictionary<string, string> parameters;

                if (string.IsNullOrEmpty(clientSecret) || clientSecret == "your-client-secret-here")
                {
                    _logger.LogInformation("Usando password grant (realm master) para obtener token de admin");
                    parameters = new Dictionary<string, string>
                    {
                        { "client_id", clientId },
                        { "username", "admin" },
                        { "password", "admin" },
                        { "grant_type", "password" }
                    };
                }
                else
                {
                    // Si hay secret, asumimos que es un client del realm Kairo (client_credentials)
                    tokenEndpoint = $"{authority}/protocol/openid-connect/token";
                    parameters = new Dictionary<string, string>
                    {
                        { "client_id", clientId },
                        { "client_secret", clientSecret },
                        { "grant_type", "client_credentials" }
                    };
                }
                
                var content = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(tokenEndpoint, content, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Error al obtener token de Keycloak. Status: {Status}, Body: {Body}", response.StatusCode, errorBody);
                    var ex = new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).");
                    ex.Data["ResponseBody"] = errorBody;
                    throw ex;
                }
                
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                _adminToken = tokenData.GetProperty("access_token").GetString();
                
                _logger.LogDebug("Token de administrador obtenido exitosamente");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error al obtener token de administrador de Keycloak");
                throw new HttpRequestException($"Error al obtener token de Keycloak: {ex.Message}", ex);
            }
        }
    }
}
