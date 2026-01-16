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

    public async Task<Dictionary<string, bool>> ConsultarEstadoProveedoresAsync(List<string> externalIds)
    {
        // Implementación simplificada para cumplir con la interfaz
        // En un escenario real, esto consultaría la disponibilidad en tiempo real
        return await Task.FromResult(new Dictionary<string, bool>());
    }

    public async Task<IEnumerable<ServicioGlobal>> ObtenerServiciosCateringAsync()
    {
        try
        {
            // El BaseAddress (http://localhost:5005/) se configurará en InyeccionDependencias
            var response = await _httpClient.GetAsync("api/servicios/catering");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            // Deserializamos usando la estructura detectada: {"servicios": [ ... ]}
            var root = JsonSerializer.Deserialize<ServiciosExternosResponse>(json, options);

            if (root?.Servicios == null) return Enumerable.Empty<ServicioGlobal>();

            // Mapeamos DTO externo -> Entidad de Dominio (Clean Architecture)
            return root.Servicios.Select(dto => new ServicioGlobal(
                Guid.NewGuid(), // Generamos un ID interno
                dto.Nombre,
                dto.Precio
            )).ToList();
        }
        catch (Exception)
        {
            // En caso de error de conexión, retornamos lista vacía para no detener el worker
            // Aquí se debería loguear el error idealmente
            return Enumerable.Empty<ServicioGlobal>();
        }
    }

    // DTOs privados para no contaminar el dominio con estructuras externas
    private class ServiciosExternosResponse
    {
        public List<ServicioExternoDto> Servicios { get; set; }
    }

    private class ServicioExternoDto
    {
        public string IdServicioExterno { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public string Moneda { get; set; }
    }
}
