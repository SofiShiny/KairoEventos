using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(HttpClient httpClient, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest("RefreshToken es requerido");
        }

        var authority = _configuration["Authentication:Authority"];
        var clientId = _configuration["Authentication:Audience"];
        var tokenEndpoint = $"{authority}/protocol/openid-connect/token";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("refresh_token", request.RefreshToken)
            // Agregado client_secret si fuera necesario en appsettings
        });

        try
        {
            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al refrescar token en Keycloak: {Response}", responseContent);
                return StatusCode((int)response.StatusCode, responseContent);
            }

            return Ok(JsonDocument.Parse(responseContent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error excepcional al llamar al endpoint de refresh");
            return StatusCode(500, "Error interno al contactar con el servidor de identidad");
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest("RefreshToken es requerido para el logout");
        }

        var authority = _configuration["Authentication:Authority"];
        var clientId = _configuration["Authentication:Audience"];
        var logoutEndpoint = $"{authority}/protocol/openid-connect/logout";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("refresh_token", request.RefreshToken)
            // Agregado client_secret si fuera necesario
        });

        try
        {
            var response = await _httpClient.PostAsync(logoutEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error al cerrar sesión en Keycloak: {Response}", responseContent);
                return StatusCode((int)response.StatusCode, responseContent);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error excepcional al llamar al endpoint de logout");
            return StatusCode(500, "Error interno al procesar el cierre de sesión");
        }
    }
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
