using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using MediatR;

namespace Eventos.Aplicacion.Comandos;

public class EliminarEventoComandoHandler : IRequestHandler<EliminarEventoComando, Resultado>
{
 private readonly IRepositorioEvento _repositorioEvento;

 public EliminarEventoComandoHandler(IRepositorioEvento repositorioEvento)
 {
 _repositorioEvento = repositorioEvento;
 }

 public async Task<Resultado> Handle(EliminarEventoComando request, CancellationToken cancellationToken)
 {
 var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, cancellationToken);
 if (evento == null) return Resultado.Falla("Evento no encontrado");
 await _repositorioEvento.EliminarAsync(request.EventoId, cancellationToken);
 return Resultado.Exito();
 }
}