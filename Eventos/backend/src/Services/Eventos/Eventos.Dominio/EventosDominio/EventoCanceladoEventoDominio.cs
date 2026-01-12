using BloquesConstruccion.Dominio;

namespace Eventos.Dominio.EventosDeDominio;

public class EventoCanceladoEventoDominio : EventoDominio
{
 public Guid EventoId { get; }
 public string TituloEvento { get; }

 public EventoCanceladoEventoDominio(Guid eventoId, string tituloEvento)
 {
 EventoId = eventoId;
 TituloEvento = tituloEvento;
 }
}
