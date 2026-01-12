using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Servicios.Dominio.Repositorios;

namespace Servicios.Infraestructura.ServiciosExternos;

public class VerificadorEntradasHttp : IVerificadorEntradas
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VerificadorEntradasHttp> _logger;

    public VerificadorEntradasHttp(HttpClient httpClient, ILogger<VerificadorEntradasHttp> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> UsuarioTieneEntradaParaEventoAsync(Guid usuarioId, Guid eventoId)
    {
        try
        {
            // Consultamos las entradas del usuario
            var response = await _httpClient.GetAsync($"/api/entradas/usuario/{usuarioId}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al consultar Entradas.API para usuario {UsuarioId}. Status: {Code}", usuarioId, response.StatusCode);
                return false;
            }

            var entradas = await response.Content.ReadFromJsonAsync<IEnumerable<EntradaTicketDto>>();
            
            // Verificamos si alguna corresponde al evento solicitado
            return entradas?.Any(e => e.EventoId == eventoId) ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al contactar a Entradas.API para usuario {UsuarioId}", usuarioId);
            return false;
        }
    }
}

// DTO interno para mapear la respuesta de Entradas.API
public class EntradaTicketDto
{
    public Guid EventoId { get; set; }
}
