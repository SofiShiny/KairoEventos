using MassTransit;
using Microsoft.EntityFrameworkCore;
using Servicios.Aplicacion.Eventos;
using Servicios.Dominio.Interfaces;
using Servicios.Infraestructura.Persistencia;

namespace Servicios.API.Workers;

public class SincronizacionWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SincronizacionWorker> _logger;
    // Petición cada 2 minutos como solicitado
    private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(2);

    public SincronizacionWorker(
        IServiceProvider serviceProvider, 
        ILogger<SincronizacionWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SincronizacionWorker iniciado. Intervalo: {Intervalo}", _intervalo);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SincronizarProveedoresAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante la sincronización de proveedores.");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private async Task SincronizarProveedoresAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        // Obtenemos los servicios necesarios
        var proveedorExternoService = scope.ServiceProvider.GetRequiredService<IProveedorExternoService>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
        // 1. Consultar a la API Externa (Adaptada)
        _logger.LogInformation("Consultando API Externa de Catering...");
        var serviciosExternos = await proveedorExternoService.ObtenerServiciosCateringAsync();
        var listaServicios = serviciosExternos.ToList();

        if (listaServicios.Any())
        {
            _logger.LogInformation("Se obtuvieron {Cantidad} servicios externos.", listaServicios.Count);

            // 2. Aquí iría la lógica de persistencia/actualización en BD (omitida para brevedad del ejercicio)
            // Ejemplo: foreach(var s in listaServicios) { ... context.AddOrUpdate(s) ... }

            // 3. Publicar evento en RabbitMQ como solicitado
            await publishEndpoint.Publish(new ServiciosCateringSincronizadosEvento(
                listaServicios.Count,
                DateTime.UtcNow
            ), stoppingToken);
            
            _logger.LogInformation("Evento de sincronización publicado en RabbitMQ.");
        }
        else
        {
            _logger.LogWarning("La API Externa no retornó servicios o hubo un error controlado.");
        }
    }
}
