using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Permitimos acceso anónimo para el panel de supervisión por ahora
public class SupervisionController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public SupervisionController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var clusters = _configuration.GetSection("ReverseProxy:Clusters").GetChildren();
        var results = new List<object>();

        using var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(3);

        foreach (var cluster in clusters)
        {
            var clusterId = cluster.Key;
            var destination = cluster.GetSection("Destinations").GetChildren().FirstOrDefault();
            
            if (destination == null) continue;

            var address = destination.GetValue<string>("Address");
            if (string.IsNullOrEmpty(address)) continue;

            // Intentamos /health primero, si no /health/live
            var healthUrl = $"{address}/health";
            
            var status = "desconocido";
            var responseTime = 0L;
            var description = GetDescription(clusterId);

            try
            {
                var sw = Stopwatch.StartNew();
                var response = await client.GetAsync(healthUrl);
                sw.Stop();
                responseTime = sw.ElapsedMilliseconds;

                if (response.IsSuccessStatusCode)
                {
                    status = "saludable";
                }
                else
                {
                    // Algunos servicios pueden no tener /health mapeado pero responder 404
                    // Intentamos una petición simple a la raíz o asumimos degradado si responde algo
                    status = "degradado";
                }
            }
            catch (Exception)
            {
                status = "caido";
            }

            results.Add(new
            {
                nombre = Capitalize(clusterId.Replace("-cluster", "")),
                descripcion = description,
                estado = status,
                tiempoRespuesta = responseTime,
                puerto = address.Split(':').LastOrDefault(),
                version = "1.0.0", // Podríamos obtenerla de un endpoint de info
                uptime = 0 // Esto requeriría más lógica
            });
        }

        return Ok(results);
    }

    private string GetDescription(string clusterId) => clusterId switch
    {
        "eventos-cluster" => "Gestión de catálogo de eventos",
        "asientos-cluster" => "Control de mapas y reservas de asientos",
        "usuarios-cluster" => "Gestión de perfiles y seguridad",
        "entradas-cluster" => "Venta y validación de tickets",
        "pagos-cluster" => "Procesamiento de pagos y pasarelas",
        "servicios-cluster" => "Servicios complementarios (Catering, etc.)",
        "notificaciones-cluster" => "Alertas y correos en tiempo real",
        "streaming-cluster" => "Transmisiones en vivo de eventos",
        "reportes-cluster" => "Analítica y reportes de ventas",
        _ => "Microservicio del sistema Kairo"
    };

    private string Capitalize(string text) => 
        string.IsNullOrEmpty(text) ? text : char.ToUpper(text[0]) + text.Substring(1);
}
