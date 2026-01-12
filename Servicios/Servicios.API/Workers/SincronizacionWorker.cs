using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Servicios.Aplicacion.Eventos;
using Servicios.Dominio.Interfaces;
using Servicios.Infraestructura.Persistencia;

namespace Servicios.API.Workers;

public class SincronizacionWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SincronizacionWorker> _logger;
    private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(30);

    public SincronizacionWorker(
        IServiceProvider serviceProvider, 
        ILogger<SincronizacionWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SincronizacionWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SincronizarProveedoresAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la sincronización de proveedores.");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private async Task SincronizarProveedoresAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ServiciosDbContext>();
        var proveedorExternoService = scope.ServiceProvider.GetRequiredService<IProveedorExternoService>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var proveedores = await context.ProveedoresServicios.ToListAsync(stoppingToken);
        if (!proveedores.Any()) return;

        var externalIds = proveedores.Select(p => p.ExternalId).ToList();
        var estadosActualizados = await proveedorExternoService.ConsultarEstadoProveedoresAsync(externalIds);

        foreach (var proveedor in proveedores)
        {
            if (estadosActualizados.TryGetValue(proveedor.ExternalId, out bool nuevoEstado))
            {
                if (proveedor.EstaDisponible != nuevoEstado)
                {
                    _logger.LogInformation("Cambio de disponibilidad detectado para {Proveedor}: {EstadoAnterior} -> {NuevoEstado}", 
                        proveedor.NombreProveedor, proveedor.EstaDisponible, nuevoEstado);

                    proveedor.SetDisponibilidad(nuevoEstado);
                    
                    await publishEndpoint.Publish(new ProveedorEstadoCambiadoEvent(
                        proveedor.ServicioId,
                        proveedor.NombreProveedor,
                        nuevoEstado
                    ), stoppingToken);
                }
            }
        }

        await context.SaveChangesAsync(stoppingToken);
    }
}
