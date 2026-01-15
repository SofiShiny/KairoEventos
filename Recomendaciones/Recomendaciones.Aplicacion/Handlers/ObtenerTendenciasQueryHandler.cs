using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using System.Net.Http.Json;
using Recomendaciones.Aplicacion.DTOs;
using Recomendaciones.Aplicacion.Queries;

namespace Recomendaciones.Aplicacion.Handlers;

/// <summary>
/// Handler para obtener eventos tendencia (top ventas)
/// </summary>
public class ObtenerTendenciasQueryHandler : IRequestHandler<ObtenerTendenciasQuery, List<EventoRecomendadoDto>>
{
    private readonly ILogger<ObtenerTendenciasQueryHandler> _logger;
    private readonly HttpClient _httpClient;

    public ObtenerTendenciasQueryHandler(
        ILogger<ObtenerTendenciasQueryHandler> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("EventosApi");
    }

    public async Task<List<EventoRecomendadoDto>> Handle(ObtenerTendenciasQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo tendencias - Top {Limite} eventos", request.Limite);

            // TODO: En producción, esto debería consultar una vista materializada o agregación de ventas
            // Por ahora, obtenemos eventos publicados y simulamos popularidad
            
            var response = await _httpClient.GetAsync("/api/eventos/publicados", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("No se pudieron obtener eventos publicados");
                return GenerarEventosMock();
            }

            var eventos = await response.Content.ReadFromJsonAsync<List<EventoInternoDto>>(cancellationToken);
            
            if (eventos == null || !eventos.Any())
            {
                return GenerarEventosMock();
            }

            // Simular popularidad basada en:
            // 1. Número de asistentes actuales
            // 2. Proximidad de la fecha (eventos próximos son más populares)
            // 3. Categoría (algunas categorías son más populares)
            
            var eventosConScore = eventos.Select(e => new
            {
                Evento = e,
                Score = CalcularScorePopularidad(e)
            })
            .OrderByDescending(x => x.Score)
            .Take(request.Limite)
            .Select(x => MapearAEventoRecomendado(x.Evento, x.Score))
            .ToList();

            _logger.LogInformation("Tendencias obtenidas: {Count} eventos", eventosConScore.Count);
            return eventosConScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tendencias");
            return GenerarEventosMock();
        }
    }

    private int CalcularScorePopularidad(EventoInternoDto evento)
    {
        int score = 0;

        // Puntos por asistentes registrados (simulando ventas)
        score += evento.ConteoAsistentesActual * 10;

        // Puntos por proximidad (eventos próximos son más atractivos)
        var diasHastaEvento = (evento.FechaInicio - DateTime.UtcNow).Days;
        if (diasHastaEvento >= 0 && diasHastaEvento <= 30)
        {
            score += (30 - diasHastaEvento) * 2; // Más puntos si está más cerca
        }

        // Puntos por categoría popular
        var categoriasPopulares = new[] { "Concierto", "Música", "Festival", "Deportes" };
        if (categoriasPopulares.Contains(evento.Categoria, StringComparer.OrdinalIgnoreCase))
        {
            score += 20;
        }

        // Bonus por capacidad alta (eventos grandes suelen ser más populares)
        if (evento.MaximoAsistentes > 500)
        {
            score += 15;
        }

        return score;
    }

    private EventoRecomendadoDto MapearAEventoRecomendado(EventoInternoDto evento, int entradasVendidas)
    {
        return new EventoRecomendadoDto
        {
            Id = evento.Id,
            Titulo = evento.Titulo ?? "Evento sin título",
            Descripcion = evento.Descripcion,
            Categoria = evento.Categoria,
            FechaInicio = evento.FechaInicio,
            FechaFin = evento.FechaFin,
            UrlImagen = evento.UrlImagen,
            NombreLugar = evento.Ubicacion?.NombreLugar,
            Ciudad = evento.Ubicacion?.Ciudad,
            EntradasVendidas = evento.ConteoAsistentesActual,
            PrecioDesde = 0 // TODO: Obtener precio mínimo de asientos
        };
    }

    private List<EventoRecomendadoDto> GenerarEventosMock()
    {
        // Datos de respaldo en caso de que el servicio de eventos no esté disponible
        return new List<EventoRecomendadoDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Titulo = "Festival de Música Electrónica 2026",
                Descripcion = "Los mejores DJs del mundo en un solo lugar",
                Categoria = "Música",
                FechaInicio = DateTime.UtcNow.AddDays(15),
                FechaFin = DateTime.UtcNow.AddDays(15).AddHours(8),
                UrlImagen = "/imagenes/default-music.jpg",
                NombreLugar = "Estadio Nacional",
                Ciudad = "Santiago",
                EntradasVendidas = 1250,
                PrecioDesde = 45000
            },
            new()
            {
                Id = Guid.NewGuid(),
                Titulo = "Conferencia Tech Summit 2026",
                Descripcion = "El futuro de la tecnología",
                Categoria = "Tecnología",
                FechaInicio = DateTime.UtcNow.AddDays(20),
                FechaFin = DateTime.UtcNow.AddDays(22),
                UrlImagen = "/imagenes/default-tech.jpg",
                NombreLugar = "Centro de Convenciones",
                Ciudad = "Santiago",
                EntradasVendidas = 850,
                PrecioDesde = 75000
            }
        };
    }
}
