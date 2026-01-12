using BloquesConstruccion.Aplicacion.Comun;
using BloquesConstruccion.Aplicacion.Comandos;
using Eventos.Dominio.Entidades;
using Eventos.Aplicacion.DTOs;

namespace Eventos.Aplicacion.Comandos;

internal static class EventoDtoMapper
{
  public static EventoDto Map(Evento e)
  {
  // Se asume que e.Ubicacion no es null debido a validaciones previas
  var u = e.Ubicacion;
  var dto = new EventoDto
  {
  Id = e.Id,
  Titulo = e.Titulo,
  Descripcion = e.Descripcion,
  Ubicacion = new UbicacionDto
  {
  NombreLugar = u.NombreLugar,
  Direccion = u.Direccion,
  Ciudad = u.Ciudad,
  Region = u.Region,
  CodigoPostal = u.CodigoPostal,
  Pais = u.Pais
  },
  FechaInicio = e.FechaInicio,
  FechaFin = e.FechaFin,
  MaximoAsistentes = e.MaximoAsistentes,
  ConteoAsistentesActual = e.ConteoAsistentesActual,
  Estado = e.Estado.ToString(),
  OrganizadorId = e.OrganizadorId,
  UrlImagen = e.UrlImagen,
  CreadoEn = e.CreadoEn,
  Asistentes = e.Asistentes.Any()
  ? e.Asistentes.Select(a => new AsistenteDto
  {
  Id = a.Id,
  NombreUsuario = a.NombreUsuario,
  Correo = a.Correo,
  RegistradoEn = a.RegistradoEn
  }).ToList()
  : null
  };
  return dto;
  }
}
