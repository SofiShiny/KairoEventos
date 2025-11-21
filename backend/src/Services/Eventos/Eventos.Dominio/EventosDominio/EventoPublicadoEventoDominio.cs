using BloquesConstruccion.Dominio;

namespace Eventos.Dominio.EventosDeDominio;

public class EventoPublicadoEventoDominio : EventoDominio
{
 public Guid EventoId { get; }
 public string TituloEvento { get; }
 public DateTime FechaInicio { get; }

 public EventoPublicadoEventoDominio(Guid eventoId, string tituloEvento, DateTime fechaInicio)
 {
 EventoId = eventoId;
 TituloEvento = tituloEvento;
 FechaInicio = fechaInicio;
 }
}
