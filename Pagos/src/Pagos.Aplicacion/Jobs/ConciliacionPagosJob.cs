using Microsoft.Extensions.Logging;
using MassTransit;
using Pagos.Dominio.Interfaces;
using Pagos.Dominio.Modelos;
using Pagos.Aplicacion.Eventos;

namespace Pagos.Aplicacion.Jobs;

public class ConciliacionPagosJob
{
    private readonly IRepositorioTransacciones _repositorio;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ConciliacionPagosJob> _logger;

    public ConciliacionPagosJob(
        IRepositorioTransacciones repositorio,
        IPublishEndpoint publishEndpoint,
        ILogger<ConciliacionPagosJob> logger)
    {
        _repositorio = repositorio;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task ConciliarPendientesAsync()
    {
        _logger.LogInformation("Ejecutando proceso de conciliación de pagos pendientes...");

        var transacciones = await _repositorio.ObtenerTodasAsync();
        
        // Filtrar pendientes con más de 15 minutos
        var pendientes = transacciones
            .Where(t => t.Estado == EstadoTransaccion.Pendiente && 
                        t.FechaCreacion < DateTime.UtcNow.AddMinutes(-15))
            .ToList();

        _logger.LogInformation("Se encontraron {Count} transacciones pendientes para conciliar", pendientes.Count);

        var random = new Random();

        foreach (var tx in pendientes)
        {
            // Simulación: 80% fallido, 20% aprobado
            var esExitoso = random.Next(1, 101) <= 20;

            if (esExitoso)
            {
                _logger.LogInformation("Conciliación: Transacción {Id} APROBADA simuladamente", tx.Id);
                
                tx.Aprobar("https://storage.eventos.com/facturas/conciliada_" + tx.Id + ".pdf");
                await _repositorio.ActualizarAsync(tx);

                // Publicar evento de integración
                await _publishEndpoint.Publish(new PagoAprobadoEvento(
                    tx.Id,
                    tx.OrdenId,
                    tx.UsuarioId,
                    tx.Monto,
                    tx.UrlFactura!
                ));
            }
            else
            {
                _logger.LogWarning("Conciliación: Transacción {Id} MARCADA COMO FALLIDA tras timeout", tx.Id);
                
                tx.Rechazar("Timeout de pasarela - Conciliado automáticamente como fallido");
                await _repositorio.ActualizarAsync(tx);
                
                // Publicar evento de rechazo
                await _publishEndpoint.Publish(new PagoRechazadoEvento(
                    tx.Id,
                    tx.OrdenId,
                    "Timeout de pasarela"
                ));
            }
        }

        _logger.LogInformation("Proceso de conciliación finalizado");
    }
}
