using Asientos.Dominio.Repositorios;
using Microsoft.AspNetCore.SignalR;
using Asientos.Aplicacion.Hubs;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Asientos.Aplicacion.Jobs;

public class LiberarAsientoJob
{
    private readonly IRepositorioMapaAsientos _repo;
    private readonly IHubContext<AsientosHub> _hubContext;
    private readonly ILogger<LiberarAsientoJob> _logger;

    public LiberarAsientoJob(
        IRepositorioMapaAsientos repo,
        IHubContext<AsientosHub> hubContext,
        ILogger<LiberarAsientoJob> logger)
    {
        _repo = repo;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Ejecutar(Guid mapaId, Guid asientoId)
    {
        try
        {
            var mapa = await _repo.ObtenerPorIdAsync(mapaId, CancellationToken.None);
            if (mapa == null)
            {
                _logger.LogWarning("Job LiberarAsiento: Mapa {MapaId} no encontrado", mapaId);
                return;
            }

            var asiento = mapa.Asientos.FirstOrDefault(a => a.Id == asientoId);
            if (asiento == null)
            {
                _logger.LogWarning("Job LiberarAsiento: Asiento {AsientoId} no encontrado en mapa {MapaId}", asientoId, mapaId);
                return;
            }

            // Lógica crítica: Solo liberar si está Reservado Y NO ha sido Pagado
            // Esto evita liberar un asiento que ya completó su flujo de compra
            if (asiento.Reservado && !asiento.Pagado)
            {
                _logger.LogInformation("Expiración de reserva: Liberando asiento {AsientoId} (Fila {Fila}-{Numero})", 
                    asientoId, asiento.Fila, asiento.Numero);
                
                // Usamos el método de dominio para liberar
                mapa.LiberarAsientoPorId(asientoId);
                
                // Persistir cambios
                await _repo.ActualizarAsync(mapa, CancellationToken.None);
                await _repo.GuardarCambiosAsync(CancellationToken.None);

                // Notificar en tiempo real via SignalR para actualizar mapas de usuarios conectados
                await _hubContext.Clients.Group($"Evento_{mapa.EventoId}")
                    .SendAsync("AsientoLiberado", asientoId);
            }
            else
            {
                 // Si está Pagado, o ya no está Reservado (fue liberado manualmente), no hacemos nada.
                 if(asiento.Pagado)
                    _logger.LogInformation("Job LiberarAsiento: Asiento {AsientoId} ya fue PAGADO. Se cancela expiración.", asientoId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando liberación de asiento {AsientoId}", asientoId);
            throw; // Reintentar job
        }
    }
}
