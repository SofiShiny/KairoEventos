using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notificaciones.Dominio.Interfaces;

namespace Notificaciones.Infraestructura.Servicios;

public class ServicioUsuariosHttp : IServicioUsuarios
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ServicioUsuariosHttp> _logger;
    private readonly string _baseUrl;

    public ServicioUsuariosHttp(HttpClient httpClient, IConfiguration configuration, ILogger<ServicioUsuariosHttp> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["UsuariosApi:Url"]?.TrimEnd('/') ?? "http://usuarios-api:8080";
    }

    public async Task<string?> ObtenerEmailUsuarioAsync(Guid usuarioId)
    {
        try
        {
            _logger.LogInformation("Consultando email para usuario {UsuarioId} en {Url}", usuarioId, _baseUrl);
            
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/Usuarios/{usuarioId}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("No se pudo obtener el usuario {UsuarioId}. Status: {Status}", usuarioId, response.StatusCode);
                return null;
            }

            var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
            return usuario?.Correo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error llamando al microservicio de usuarios para {UsuarioId}", usuarioId);
            return null;
        }
    }
}

public class UsuarioResponse
{
    public string Correo { get; set; } = string.Empty;
}
