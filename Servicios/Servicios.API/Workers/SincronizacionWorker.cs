using MassTransit;
using Microsoft.EntityFrameworkCore;
using Servicios.Aplicacion.Eventos;
using Servicios.Dominio.Interfaces;
using Servicios.Dominio.Repositorios;
using Servicios.Dominio.Entidades;
using Servicios.Infraestructura.Persistencia;

namespace Servicios.API.Workers;

public class SincronizacionWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SincronizacionWorker> _logger;
    // Petición cada 10 segundos para pruebas rápidas
    private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(10);

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
        string[] tipos = { "transporte", "catering", "merchandising" };
        int totalSincronizados = 0;

        foreach (var tipo in tipos)
        {
            try 
            {
                using var scope = _serviceProvider.CreateScope();
                var proveedorExternoService = scope.ServiceProvider.GetRequiredService<IProveedorExternoService>();
                var repositorio = scope.ServiceProvider.GetRequiredService<IRepositorioServicios>();

                _logger.LogInformation("Consultando API Externa para: {Tipo}...", tipo);
                var serviciosExternos = await proveedorExternoService.ObtenerServiciosPorTipoAsync(tipo);
                var listaServicios = serviciosExternos.ToList();

                if (!listaServicios.Any()) 
                {
                    _logger.LogInformation("No hay servicios externos para el tipo: {Tipo}", tipo);
                    continue;
                }

                // Buscamos el servicio global que corresponde a este tipo (por nombre)
                var servicioGlobal = await repositorio.ObtenerServicioPorNombreAsync(tipo);
                if (servicioGlobal == null)
                {
                    _logger.LogWarning("No se encontró un ServicioGlobal para el tipo: {Tipo}", tipo);
                    continue;
                }

                foreach (var ext in listaServicios)
                {
                    var proveedorExistente = servicioGlobal.Proveedores
                        .FirstOrDefault(p => p.ExternalId == ext.IdServicioExterno);

                    if (proveedorExistente != null)
                    {
                        proveedorExistente.SetDisponibilidad(ext.Disponible);
                        proveedorExistente.ActualizarPrecio(ext.Precio);
                    }
                    else
                    {
                        // Usar Guid.Empty para INSERT
                        var nuevo = new ProveedorServicio(Guid.Empty, servicioGlobal.Id, ext.Nombre, ext.Precio, ext.IdServicioExterno);
                        nuevo.SetDisponibilidad(ext.Disponible);
                        servicioGlobal.AgregarProveedor(nuevo);
                    }
                }

                if (listaServicios.Count == 1)
                {
                    servicioGlobal.ActualizarPrecio(listaServicios[0].Precio);
                }

                await repositorio.SaveAsync();
                totalSincronizados += listaServicios.Count;
                _logger.LogInformation("Sincronizados {Cant} proveedores para {Tipo}", listaServicios.Count, tipo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sincronizando tipo {Tipo}", tipo);
            }
        }

        if (totalSincronizados > 0)
        {
            using var scope = _serviceProvider.CreateScope();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await publishEndpoint.Publish(new ServiciosCateringSincronizadosEvento(
                totalSincronizados,
                DateTime.UtcNow
            ), stoppingToken);
        }
    }
}
