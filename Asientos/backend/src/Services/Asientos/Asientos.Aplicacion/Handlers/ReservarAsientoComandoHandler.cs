using MediatR;
using Microsoft.AspNetCore.SignalR;
using MassTransit;
using Asientos.Aplicacion.Comandos;
using Asientos.Dominio.Repositorios;
using Asientos.Dominio.EventosDominio;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Asientos.Aplicacion.Handlers;

public class ReservarAsientoComandoHandler : IRequestHandler<ReservarAsientoComando, Unit>
{
    private readonly IRepositorioMapaAsientos _repo;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHubContext<Asientos.Aplicacion.Hubs.AsientosHub> _hubContext;

    public ReservarAsientoComandoHandler(
        IRepositorioMapaAsientos repo, 
        IPublishEndpoint publishEndpoint,
        IHubContext<Asientos.Aplicacion.Hubs.AsientosHub> hubContext)
    {
        _repo = repo;
        _publishEndpoint = publishEndpoint;
        _hubContext = hubContext;
    }

    public async Task<Unit> Handle(ReservarAsientoComando request, CancellationToken cancellationToken)
    {
        var mapa = await _repo.ObtenerPorIdAsync(request.MapaId, cancellationToken) 
                   ?? throw new InvalidOperationException("Mapa no existe");
        
        mapa.ReservarAsientoPorId(request.AsientoId, request.UsuarioId);
        
        await _repo.ActualizarAsync(mapa, cancellationToken);

        // Notificar en tiempo real via SignalR
        await _hubContext.Clients.Group($"Evento_{mapa.EventoId}")
            .SendAsync("AsientoReservado", request.AsientoId, request.UsuarioId);

        // Opcional: El evento de dominio ya fue generado en la entidad, 
        // podrías iterar mapa.EventosDominio y publicarlos.
        // Por ahora mantengo el publish manual para simplificar compatibilidad.
        var asiento = mapa.Asientos.First(a => a.Id == request.AsientoId);
        await _publishEndpoint.Publish(new AsientoReservadoEventoDominio(request.MapaId, asiento.Fila, asiento.Numero), cancellationToken);

        return Unit.Value;
    }
}
