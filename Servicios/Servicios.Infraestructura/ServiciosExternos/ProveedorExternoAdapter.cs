using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Servicios.Dominio.Entidades;
using Servicios.Dominio.Interfaces;

namespace Servicios.Infraestructura.ServiciosExternos;

public class ProveedorExternoAdapter : IProveedorExternoService
{
    private readonly HttpClient _httpClient;

    public ProveedorExternoAdapter(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IEnumerable<ServicioExternoDto>> ObtenerServiciosPorTipoAsync(string tipo)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/servicios/{tipo}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            var root = JsonSerializer.Deserialize<ServiciosExternosResponse>(json, options);

            return root?.Servicios ?? Enumerable.Empty<ServicioExternoDto>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<ServicioExternoDto>();
        }
    }

    public async Task<IEnumerable<ServicioExternoDto>> ObtenerTodosLosServiciosAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/servicios");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            return JsonSerializer.Deserialize<List<ServicioExternoDto>>(json, options) ?? Enumerable.Empty<ServicioExternoDto>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<ServicioExternoDto>();
        }
    }

    public async Task ActualizarServicioAsync(string idExterno, decimal precio, bool disponible)
    {
        var request = new { IdServicioExterno = idExterno, Precio = precio, Disponible = disponible };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/servicios/update", content);
        response.EnsureSuccessStatusCode();
    }

    private class ServiciosExternosResponse
    {
        public List<ServicioExternoDto> Servicios { get; set; } = new();
    }
}
