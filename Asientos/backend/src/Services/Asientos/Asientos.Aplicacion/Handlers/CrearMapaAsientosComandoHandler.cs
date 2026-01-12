using MediatR;
using MassTransit;
using Asientos.Aplicacion.Comandos;
using Asientos.Dominio.Agregados;
using Asientos.Dominio.Repositorios;
using Asientos.Dominio.EventosDominio;

namespace Asientos.Aplicacion.Handlers;

public class CrearMapaAsientosComandoHandler : IRequestHandler<CrearMapaAsientosComando, Guid>
{
 private readonly IRepositorioMapaAsientos _repo;
 private readonly IPublishEndpoint _publishEndpoint;
 
 public CrearMapaAsientosComandoHandler(IRepositorioMapaAsientos repo, IPublishEndpoint publishEndpoint)
 {
  _repo = repo;
  _publishEndpoint = publishEndpoint;
 }
 
 public async Task<Guid> Handle(CrearMapaAsientosComando request, CancellationToken cancellationToken)
 {
  var mapa = MapaAsientos.Crear(request.EventoId);
  await _repo.AgregarAsync(mapa, cancellationToken);
  
  // Publicar evento a RabbitMQ
  await _publishEndpoint.Publish(new MapaAsientosCreadoEventoDominio(mapa.Id, request.EventoId), cancellationToken);
  
  return mapa.Id;
 }
}
