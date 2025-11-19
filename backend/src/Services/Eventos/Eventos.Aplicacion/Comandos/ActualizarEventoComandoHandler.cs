using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Aplicacion.DTOs;
using MediatR;

namespace Eventos.Aplicacion.Comandos;

public class ActualizarEventoComandoHandler : IRequestHandler<ActualizarEventoComando, Resultado<EventoDto>>
{
 private readonly IRepositorioEvento _repositorioEvento;

 public ActualizarEventoComandoHandler(IRepositorioEvento repositorioEvento)
 {
 _repositorioEvento = repositorioEvento;
 }

 public async Task<Resultado<EventoDto>> Handle(ActualizarEventoComando request, CancellationToken cancellationToken)
 {
 var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, cancellationToken);
 if (evento == null) return Resultado<EventoDto>.Falla("Evento no encontrado");

 var nuevaUbicacion = request.Ubicacion is null ? evento.Ubicacion : new Ubicacion(
 request.Ubicacion.NombreLugar ?? evento.Ubicacion.NombreLugar,
 request.Ubicacion.Direccion ?? evento.Ubicacion.Direccion,
 request.Ubicacion.Ciudad ?? evento.Ubicacion.Ciudad,
 request.Ubicacion.Region ?? evento.Ubicacion.Region,
 request.Ubicacion.CodigoPostal ?? evento.Ubicacion.CodigoPostal,
 request.Ubicacion.Pais ?? evento.Ubicacion.Pais);

 evento.Actualizar(
 request.Titulo ?? evento.Titulo,
 request.Descripcion ?? evento.Descripcion,
 nuevaUbicacion,
 request.FechaInicio ?? evento.FechaInicio,
 request.FechaFin ?? evento.FechaFin,
 request.MaximoAsistentes ?? evento.MaximoAsistentes);

 await _repositorioEvento.ActualizarAsync(evento, cancellationToken);

 var dto = new EventoDto
 {
 Id = evento.Id,
 Titulo = evento.Titulo,
 Descripcion = evento.Descripcion,
 Ubicacion = new UbicacionDto
 {
 NombreLugar = evento.Ubicacion.NombreLugar,
 Direccion = evento.Ubicacion.Direccion,
 Ciudad = evento.Ubicacion.Ciudad,
 Region = evento.Ubicacion.Region,
 CodigoPostal = evento.Ubicacion.CodigoPostal,
 Pais = evento.Ubicacion.Pais
 },
 FechaInicio = evento.FechaInicio,
 FechaFin = evento.FechaFin,
 MaximoAsistentes = evento.MaximoAsistentes,
 ConteoAsistentesActual = evento.ConteoAsistentesActual,
 Estado = evento.Estado.ToString(),
 OrganizadorId = evento.OrganizadorId,
 CreadoEn = evento.CreadoEn,
 Asistentes = evento.Asistentes.Select(a => new AsistenteDto
 {
 Id = a.Id,
 NombreUsuario = a.NombreUsuario,
 Correo = a.Correo,
 RegistradoEn = a.RegistradoEn
 })
 };

 return Resultado<EventoDto>.Exito(dto);
 }
}