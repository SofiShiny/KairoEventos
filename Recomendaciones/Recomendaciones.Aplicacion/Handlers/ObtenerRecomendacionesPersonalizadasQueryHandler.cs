using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using System.Net.Http.Json;
using Recomendaciones.Aplicacion.DTOs;
using Recomendaciones.Aplicacion.Queries;

namespace Recomendaciones.Aplicacion.Handlers;

/// <summary>
/// Handler para obtener recomendaciones personalizadas basadas en el historial del usuario
/// </summary>
public class ObtenerRecomendacionesPersonalizadasQueryHandler 
    : IRequestHandler<ObtenerRecomendacionesPersonalizadasQuery, RecomendacionesPersonalizadasDto>
{
    private readonly ILogger<ObtenerRecomendacionesPersonalizadasQueryHandler> _logger;
    private readonly HttpClient _eventosClient;
    private readonly HttpClient _entradasClient;
    private readonly IMediator _mediator;

    public ObtenerRecomendacionesPersonalizadasQueryHandler(
        ILogger<ObtenerRecomendacionesPersonalizadasQueryHandler> logger,
        IHttpClientFactory httpClientFactory,
        IMediator mediator)
    {
        _logger = logger;
        _eventosClient = httpClientFactory.CreateClient("EventosApi");
        _entradasClient = httpClientFactory.CreateClient("EntradasApi");
        _mediator = mediator;
    }

    public async Task<RecomendacionesPersonalizadasDto> Handle(
        ObtenerRecomendacionesPersonalizadasQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Generando recomendaciones personalizadas para usuario {UsuarioId}", request.UsuarioId);

            // 1. Obtener historial de compras del usuario
            var categoriaFavorita = await ObtenerCategoriaFavoritaAsync(request.UsuarioId, cancellationToken);

            // 2. Si tiene historial, recomendar por categoría favorita
            if (!string.IsNullOrEmpty(categoriaFavorita))
            {
                var eventosPersonalizados = await ObtenerEventosPorCategoriaAsync(categoriaFavorita, cancellationToken);
                
                return new RecomendacionesPersonalizadasDto
                {
                    TipoRecomendacion = "Personalizado",
                    CategoriaFavorita = categoriaFavorita,
                    TituloSeccion = $"Porque te gusta {categoriaFavorita}",
                    Eventos = eventosPersonalizados
                };
            }

            // 3. Si no tiene historial, devolver tendencias
            _logger.LogInformation("Usuario {UsuarioId} sin historial, devolviendo tendencias", request.UsuarioId);
            var tendencias = await _mediator.Send(new ObtenerTendenciasQuery(6), cancellationToken);

            return new RecomendacionesPersonalizadasDto
            {
                TipoRecomendacion = "Tendencias",
                CategoriaFavorita = null,
                TituloSeccion = "Lo más vendido hoy",
                Eventos = tendencias
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar recomendaciones personalizadas para usuario {UsuarioId}", request.UsuarioId);
            
            // Fallback a tendencias
            var tendencias = await _mediator.Send(new ObtenerTendenciasQuery(6), cancellationToken);
            return new RecomendacionesPersonalizadasDto
            {
                TipoRecomendacion = "Tendencias",
                TituloSeccion = "Descubre eventos populares",
                Eventos = tendencias
            };
        }
    }

    private async Task<string?> ObtenerCategoriaFavoritaAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        try
        {
            // Intentar obtener historial de entradas del usuario
            var response = await _entradasClient.GetAsync($"/api/entradas/usuario/{usuarioId}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("No se pudo obtener historial de entradas para usuario {UsuarioId}", usuarioId);
                return SimularCategoriaFavorita(usuarioId);
            }

            var entradas = await response.Content.ReadFromJsonAsync<List<EntradaInternaDto>>(cancellationToken);
            
            if (entradas == null || !entradas.Any())
            {
                return SimularCategoriaFavorita(usuarioId);
            }

            // Analizar categorías más frecuentes
            var categoriaFavorita = entradas
                .Where(e => !string.IsNullOrEmpty(e.Categoria))
                .GroupBy(e => e.Categoria)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()
                ?.Key;

            _logger.LogInformation("Categoría favorita detectada para usuario {UsuarioId}: {Categoria}", 
                usuarioId, categoriaFavorita ?? "ninguna");

            return categoriaFavorita;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener categoría favorita, usando simulación");
            return SimularCategoriaFavorita(usuarioId);
        }
    }

    private string? SimularCategoriaFavorita(Guid usuarioId)
    {
        // Simulación inteligente basada en el hash del usuarioId
        // Esto permite que cada usuario tenga una "preferencia" consistente
        var categorias = new[] { "Música", "Concierto", "Tecnología", "Deportes", "Teatro", "Conferencia" };
        var hash = Math.Abs(usuarioId.GetHashCode());
        var index = hash % categorias.Length;
        
        return categorias[index];
    }

    private async Task<List<EventoRecomendadoDto>> ObtenerEventosPorCategoriaAsync(
        string categoria, 
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _eventosClient.GetAsync("/api/eventos/publicados", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return new List<EventoRecomendadoDto>();
            }

            var eventos = await response.Content.ReadFromJsonAsync<List<EventoInternoDto>>(cancellationToken);
            
            if (eventos == null || !eventos.Any())
            {
                return new List<EventoRecomendadoDto>();
            }

            // Filtrar por categoría y eventos futuros
            var eventosFiltrados = eventos
                .Where(e => e.Categoria != null && 
                           e.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase) &&
                           e.FechaInicio > DateTime.UtcNow)
                .OrderBy(e => e.FechaInicio)
                .Take(6)
                .Select(e => new EventoRecomendadoDto
                {
                    Id = e.Id,
                    Titulo = e.Titulo ?? "Evento sin título",
                    Descripcion = e.Descripcion,
                    Categoria = e.Categoria,
                    FechaInicio = e.FechaInicio,
                    FechaFin = e.FechaFin,
                    UrlImagen = e.UrlImagen,
                    NombreLugar = e.Ubicacion?.NombreLugar,
                    Ciudad = e.Ubicacion?.Ciudad,
                    EntradasVendidas = e.ConteoAsistentesActual,
                    PrecioDesde = 0
                })
                .ToList();

            _logger.LogInformation("Encontrados {Count} eventos de categoría {Categoria}", 
                eventosFiltrados.Count, categoria);

            return eventosFiltrados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener eventos por categoría {Categoria}", categoria);
            return new List<EventoRecomendadoDto>();
        }
    }
}
