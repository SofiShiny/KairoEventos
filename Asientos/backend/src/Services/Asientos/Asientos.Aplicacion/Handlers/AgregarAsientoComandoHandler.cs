using MediatR;
using MassTransit;
using Asientos.Aplicacion.Comandos;
using Asientos.Dominio.Repositorios;
using Asientos.Dominio.EventosDominio;

namespace Asientos.Aplicacion.Handlers;

public class AgregarAsientoComandoHandler : IRequestHandler<AgregarAsientoComando, Guid>
{
 private readonly IRepositorioMapaAsientos _repo;
 private readonly IPublishEndpoint _publishEndpoint;
 
 public AgregarAsientoComandoHandler(IRepositorioMapaAsientos repo, IPublishEndpoint publishEndpoint)
 {
  _repo = repo;
  _publishEndpoint = publishEndpoint;
 }
 
 public async Task<Guid> Handle(AgregarAsientoComando request, CancellationToken cancellationToken)
 {
  var mapa = await _repo.ObtenerPorIdAsync(request.MapaId, cancellationToken) ?? throw new InvalidOperationException("Mapa no existe");
  var asiento = mapa.AgregarAsiento(request.Fila, request.Numero, request.Categoria);
  
  // Insertar asiento directo sin actualizar root
  var id = await _repo.AgregarAsientoAsync(mapa, asiento, cancellationToken);
  
  // Publicar evento a RabbitMQ
  await _publishEndpoint.Publish(new AsientoAgregadoEventoDominio(request.MapaId, request.Fila, request.Numero, request.Categoria), cancellationToken);
  
  return id;
 }
}
