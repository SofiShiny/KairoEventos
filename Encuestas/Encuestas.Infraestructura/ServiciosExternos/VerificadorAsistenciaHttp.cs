using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Encuestas.Dominio.Repositorios;

namespace Encuestas.Infraestructura.ServiciosExternos;

public class VerificadorAsistenciaHttp : IVerificadorAsistencia
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VerificadorAsistenciaHttp> _logger;

    public VerificadorAsistenciaHttp(HttpClient httpClient, ILogger<VerificadorAsistenciaHttp> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> VerificarAsistenciaAsync(Guid usuarioId, Guid eventoId)
    {
        try
        {
            // Consultamos todas las entradas del usuario
            var response = await _httpClient.GetAsync($"/api/entradas/usuario/{usuarioId}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al consultar Entradas.API: {StatusCode}", response.StatusCode);
                return false;
            }

            var entradas = await response.Content.ReadFromJsonAsync<IEnumerable<EntradaTicketDto>>();
            
            if (entradas == null) return false;

            // Buscamos si el usuario tiene una entrada para ESTE evento en estado 'Usada' (valor 4 en el enum)
            return entradas.Any(e => e.EventoId == eventoId && (e.Estado?.ToString() == "4" || e.Estado?.ToString() == "Usada"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al contactar Entradas.API");
            return false;
        }
    }
}

// DTO interno para mapear la respuesta
public class EntradaTicketDto
{
    public Guid EventoId { get; set; }
    public object Estado { get; set; } = default!; // Usamos object para manejar tanto string como int
}
