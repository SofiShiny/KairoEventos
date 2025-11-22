using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Aplicacion.DTOs;
using MediatR;
using System.Linq;

namespace Eventos.Aplicacion.Comandos;

public class ActualizarEventoComandoHandler : IRequestHandler<ActualizarEventoComando, Resultado<EventoDto>>
{
 private readonly IRepositorioEvento _repositorioEvento;
 public ActualizarEventoComandoHandler(IRepositorioEvento repositorioEvento) => _repositorioEvento = repositorioEvento;

 public async Task<Resultado<EventoDto>> Handle(ActualizarEventoComando request, CancellationToken cancellationToken)
 {
 var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, cancellationToken);
 if (evento == null) return Resultado<EventoDto>.Falla("Evento no encontrado");
 var nuevaUbicacion = ConstruirUbicacion(evento, request.Ubicacion);
 evento.Actualizar(
 request.Titulo ?? evento.Titulo,
 request.Descripcion ?? evento.Descripcion,
 nuevaUbicacion,
 request.FechaInicio ?? evento.FechaInicio,
 request.FechaFin ?? evento.FechaFin,
 request.MaximoAsistentes ?? evento.MaximoAsistentes);
 await _repositorioEvento.ActualizarAsync(evento, cancellationToken);
 return Resultado<EventoDto>.Exito(EventoDtoMapper.Map(evento));
 }

 private static Ubicacion ConstruirUbicacion(Dominio.Entidades.Evento original, UbicacionDto? cambio) => cambio is null
 ? original.Ubicacion
 : new Ubicacion(
 cambio.NombreLugar ?? original.Ubicacion.NombreLugar,
 cambio.Direccion ?? original.Ubicacion.Direccion,
 cambio.Ciudad ?? original.Ubicacion.Ciudad,
 cambio.Region ?? original.Ubicacion.Region,
 cambio.CodigoPostal ?? original.Ubicacion.CodigoPostal,
 cambio.Pais ?? original.Ubicacion.Pais);
}