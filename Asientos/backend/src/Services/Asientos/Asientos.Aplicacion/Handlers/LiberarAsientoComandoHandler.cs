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

public class LiberarAsientoComandoHandler : IRequestHandler<LiberarAsientoComando, Unit>
{
    private readonly IRepositorioMapaAsientos _repo;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHubContext<Asientos.Aplicacion.Hubs.AsientosHub> _hubContext;

    public LiberarAsientoComandoHandler(
        IRepositorioMapaAsientos repo, 
        IPublishEndpoint publishEndpoint,
        IHubContext<Asientos.Aplicacion.Hubs.AsientosHub> hubContext)
    {
        _repo = repo;
        _publishEndpoint = publishEndpoint;
        _hubContext = hubContext;
    }

    public async Task<Unit> Handle(LiberarAsientoComando request, CancellationToken cancellationToken)
    {
        var mapa = await _repo.ObtenerPorIdAsync(request.MapaId, cancellationToken) 
                   ?? throw new InvalidOperationException("Mapa no existe");
        
        var asiento = mapa.Asientos.FirstOrDefault(a => a.Id == request.AsientoId)
                      ?? throw new InvalidOperationException("Asiento no existe");

        mapa.LiberarAsientoPorId(request.AsientoId);
        
        await _repo.ActualizarAsync(mapa, cancellationToken);
        
        // Notificar en tiempo real via SignalR
        await _hubContext.Clients.Group($"Evento_{mapa.EventoId}")
            .SendAsync("AsientoLiberado", request.AsientoId);
        
        await _publishEndpoint.Publish(new AsientoLiberadoEventoDominio(request.MapaId, asiento.Id, asiento.Fila, asiento.Numero), cancellationToken);

        return Unit.Value;
    }
}
