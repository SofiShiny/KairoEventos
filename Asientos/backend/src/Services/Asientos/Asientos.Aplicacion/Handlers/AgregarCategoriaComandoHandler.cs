using MediatR;
using MassTransit;
using Asientos.Aplicacion.Comandos;
using Asientos.Dominio.Repositorios;
using Asientos.Dominio.EventosDominio;

namespace Asientos.Aplicacion.Handlers;

public class AgregarCategoriaComandoHandler : IRequestHandler<AgregarCategoriaComando, Guid>
{
 private readonly IRepositorioMapaAsientos _repo;
 private readonly IPublishEndpoint _publishEndpoint;
 
 public AgregarCategoriaComandoHandler(IRepositorioMapaAsientos repo, IPublishEndpoint publishEndpoint)
 {
  _repo = repo;
  _publishEndpoint = publishEndpoint;
 }
 
 public async Task<Guid> Handle(AgregarCategoriaComando request, CancellationToken cancellationToken)
 {
  var mapa = await _repo.ObtenerPorIdAsync(request.MapaId, cancellationToken) ?? throw new InvalidOperationException("Mapa no existe");
  var cat = mapa.AgregarCategoria(request.Nombre, request.PrecioBase, request.TienePrioridad);
  await _repo.ActualizarAsync(mapa, cancellationToken);
  
  // Publicar evento a RabbitMQ
  await _publishEndpoint.Publish(new CategoriaAgregadaEventoDominio(request.MapaId, request.Nombre), cancellationToken);
  
  return Guid.NewGuid(); // sigue retornando id sintético
 }
}
