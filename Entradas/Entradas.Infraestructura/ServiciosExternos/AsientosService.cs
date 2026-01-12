using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Entradas.Dominio.Interfaces;
using Entradas.Dominio.Excepciones;

namespace Entradas.Infraestructura.ServiciosExternos;

public class AsientosService : IAsientosService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AsientosService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AsientosService(HttpClient httpClient, ILogger<AsientosService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<AsientoDto> GetAsientoByIdAsync(Guid asientoId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Consultando microservicio de Asientos para el ID: {AsientoId}", asientoId);
            
            // La URL base se configurará vía HttpClient factory
            var response = await _httpClient.GetAsync($"api/asientos/{asientoId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Asiento {AsientoId} no encontrado en el microservicio de Asientos", asientoId);
                throw new AsientoNoDisponibleException(Guid.Empty, asientoId, $"El asiento con ID {asientoId} no existe.");
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AsientoDto>(_jsonOptions, cancellationToken);

            if (result == null)
            {
                throw new Exception("Error al deserializar la respuesta del microservicio de Asientos.");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de comunicación con el microservicio de Asientos");
            throw new Exception("El servicio de Asientos no está disponible.", ex);
        }
    }
}
