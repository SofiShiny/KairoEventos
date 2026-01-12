using System.Threading.Tasks;
using Asientos.Dominio.Agregados;
using Asientos.Dominio.Repositorios;
using Entradas.Dominio.Eventos;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Asientos.Aplicacion.Consumers;

public class ReservaCanceladaConsumer : IConsumer<ReservaCanceladaEvento>
{
    private readonly IRepositorioMapaAsientos _repositorio;
    private readonly ILogger<ReservaCanceladaConsumer> _logger;

    public ReservaCanceladaConsumer(
        IRepositorioMapaAsientos repositorio,
        ILogger<ReservaCanceladaConsumer> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReservaCanceladaEvento> context)
    {
        var evento = context.Message;
        _logger.LogInformation("Procesando cancelación de reserva para Asiento: {AsientoId}, Evento: {EventoId}", 
            evento.AsientoId, evento.EventoId);

        if (!evento.AsientoId.HasValue)
        {
            _logger.LogWarning("El evento de cancelación no contiene un AsientoId. Saltando...");
            return;
        }

        var asiento = await _repositorio.ObtenerAsientoPorIdAsync(evento.AsientoId.Value, context.CancellationToken);

        if (asiento == null)
        {
            _logger.LogWarning("No se encontró el asiento {AsientoId} en la base de datos.", evento.AsientoId);
            return;
        }

        if (!asiento.Reservado)
        {
            _logger.LogInformation("El asiento {AsientoId} ya se encuentra liberado. Idempotencia aplicada.", evento.AsientoId);
            return;
        }

        // Obtener el mapa para realizar la liberación a través del agregado
        var mapa = await _repositorio.ObtenerPorIdAsync(asiento.MapaId, context.CancellationToken);
        
        if (mapa == null)
        {
            _logger.LogError("No se encontró el MapaAsientos {MapaId} asociado al asiento.", asiento.MapaId);
            return;
        }

        _logger.LogInformation("Liberando asiento {Fila}-{Numero} en mapa {MapaId}", asiento.Fila, asiento.Numero, mapa.Id);
        
        mapa.LiberarAsiento(asiento.Fila, asiento.Numero);

        await _repositorio.ActualizarAsync(mapa, context.CancellationToken);

        _logger.LogInformation("Asiento {AsientoId} liberado exitosamente por expiración.", evento.AsientoId);
    }
}
