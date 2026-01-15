using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Entradas.Dominio.Interfaces;
using Entradas.Dominio.Excepciones;

namespace Entradas.Infraestructura.ServiciosExternos;

/// <summary>
/// Implementación HTTP del verificador de eventos con políticas de resiliencia
/// </summary>
public class VerificadorEventosHttp : IVerificadorEventos
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VerificadorEventosHttp> _logger;
    private readonly VerificadorEventosOptions _options;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private readonly JsonSerializerOptions _jsonOptions;

    public VerificadorEventosHttp(
        HttpClient httpClient,
        ILogger<VerificadorEventosHttp> logger,
        IOptions<VerificadorEventosOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // Configurar políticas de resiliencia
        _retryPolicy = CreateRetryPolicy();
        
        // Configurar opciones de JSON
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // Configurar HttpClient
        ConfigurarHttpClient();
    }

    /// <inheritdoc />
    public async Task<bool> EventoExisteYDisponibleAsync(Guid eventoId, CancellationToken cancellationToken = default)
    {
        if (eventoId == Guid.Empty)
        {
            throw new ArgumentException("El ID del evento no puede ser vacío", nameof(eventoId));
        }

        var requestUri = $"{_options.BaseUrl}/api/eventos/{eventoId}/disponible";
        
        var response = await ExecuteHttpWithResilienceAsync(eventoId, 
            () => _httpClient.GetAsync(requestUri, cancellationToken), 
            "verificación de disponibilidad");
            
        var disponible = response.StatusCode == HttpStatusCode.OK;
        
        _logger.LogDebug("Evento {EventoId} disponible: {Disponible}, StatusCode: {StatusCode}", 
            eventoId, disponible, response.StatusCode);

        return disponible;
    }

    /// <inheritdoc />
    public async Task<EventoInfo?> ObtenerInfoEventoAsync(Guid eventoId, CancellationToken cancellationToken = default)
    {
        if (eventoId == Guid.Empty)
        {
            throw new ArgumentException("El ID del evento no puede ser vacío", nameof(eventoId));
        }

        var requestUri = $"{_options.BaseUrl}/api/eventos/{eventoId}";
        
        var response = await ExecuteHttpWithResilienceAsync(eventoId, 
            () => _httpClient.GetAsync(requestUri, cancellationToken), 
            "obtención de información");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Evento no encontrado: {EventoId}", eventoId);
            return null;
        }

        try
        {
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var eventoDto = JsonSerializer.Deserialize<EventoInternoDto>(content, _jsonOptions);

            if (eventoDto == null)
            {
                _logger.LogWarning("Respuesta vacía del servicio de eventos para: {EventoId}", eventoId);
                return null;
            }

            var eventoInfo = new EventoInfo(
                eventoDto.Id,
                eventoDto.Titulo ?? "Sin Nombre",
                eventoDto.FechaInicio,
                eventoDto.EstaDisponible,
                eventoDto.PrecioBase,
                eventoDto.UrlImagen,
                eventoDto.OrganizadorId,
                eventoDto.EsVirtual
            );

            _logger.LogDebug("Información del evento obtenida: {EventoId}, Nombre: {Nombre}, Disponible: {Disponible}", 
                eventoInfo.Id, eventoInfo.Nombre, eventoInfo.EstaDisponible);

            return eventoInfo;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al deserializar respuesta en obtención de información: {EventoId}", eventoId);
            throw new ServicioExternoNoDisponibleException("Eventos", 
                "Error al procesar la respuesta del servicio de eventos", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EventoRecomendado>> ObtenerSugerenciasAsync(string? categoria, int cantidad, CancellationToken cancellationToken = default)
    {
        var categoryParam = !string.IsNullOrWhiteSpace(categoria) ? $"&categoria={WebUtility.UrlEncode(categoria)}" : "";
        var requestUri = $"{_options.BaseUrl}/api/eventos/sugeridos?top={cantidad}{categoryParam}";
        
        try
        {
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync(requestUri, cancellationToken));
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al obtener sugerencias de eventos. StatusCode: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<EventoRecomendado>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var eventosDto = JsonSerializer.Deserialize<IEnumerable<EventoInternoDto>>(content, _jsonOptions);

            if (eventosDto == null) return Enumerable.Empty<EventoRecomendado>();

            return eventosDto.Select(e => new EventoRecomendado(
                e.Id,
                e.Titulo ?? "Sin título",
                e.Categoria,
                e.FechaInicio,
                e.UrlImagen
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sugerencias de eventos para categoría {Categoria}. Devolviendo lista vacía (Fallback).", categoria);
            return Enumerable.Empty<EventoRecomendado>();
        }
    }

    private async Task<HttpResponseMessage> ExecuteHttpWithResilienceAsync(Guid eventoId, Func<Task<HttpResponseMessage>> action, string context)
    {
        try
        {
            _logger.LogDebug("Iniciando petición HTTP para {Context}: {EventoId}", context, eventoId);
            return await _retryPolicy.ExecuteAsync(action);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Circuit breaker abierto para {Context}: {EventoId}", context, eventoId);
            throw new ServicioExternoNoDisponibleException("Eventos", 
                "El servicio de eventos no está disponible temporalmente", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error HTTP en {Context}: {EventoId}", context, eventoId);
            throw new ServicioExternoNoDisponibleException("Eventos", 
                "Error de comunicación con el servicio de eventos", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout en {Context}: {EventoId}", context, eventoId);
            throw new ServicioExternoNoDisponibleException("Eventos", 
                "Timeout al comunicarse con el servicio de eventos", ex);
        }
        catch (Exception ex) when (ex is not ServicioExternoNoDisponibleException && ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error inesperado en {Context}: {EventoId}", context, eventoId);
            throw new ServicioExternoNoDisponibleException("Eventos", 
                "Error inesperado al comunicarse con el servicio de eventos", ex);
        }
    }

    private IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != HttpStatusCode.NotFound)
            .WaitAndRetryAsync(
                retryCount: _options.MaxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Reintento {RetryCount} para servicio de eventos en {Delay}ms. Razón: {Reason}",
                        retryCount, timespan.TotalMilliseconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });

        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != HttpStatusCode.NotFound)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: _options.CircuitBreakerFailureThreshold,
                durationOfBreak: TimeSpan.FromSeconds(_options.CircuitBreakerDurationSeconds),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError("Circuit breaker abierto para servicio de eventos por {Duration}s. Razón: {Reason}",
                        duration.TotalSeconds, exception.Exception?.Message ?? exception.Result?.StatusCode.ToString());
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker cerrado para servicio de eventos");
                });

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }

    private void ConfigurarHttpClient()
    {
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Entradas.API/1.0");
        
        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _options.ApiKey);
        }
    }
}

/// <summary>
/// DTO para la respuesta del servicio de eventos
/// </summary>
internal record EventoInternoDto(
    Guid Id,
    string? Titulo,
    string? Categoria,
    DateTime FechaInicio,
    string? UrlImagen,
    bool EstaDisponible,
    decimal PrecioBase,
    string? OrganizadorId,
    bool EsVirtual = false
);

/// <summary>
/// Opciones de configuración para el verificador de eventos
/// </summary>
public class VerificadorEventosOptions
{
    public const string SectionName = "VerificadorEventos";

    public string BaseUrl { get; set; } = "http://localhost:5001";
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}