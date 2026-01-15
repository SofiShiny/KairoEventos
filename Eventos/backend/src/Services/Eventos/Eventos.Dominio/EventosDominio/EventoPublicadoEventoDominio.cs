using BloquesConstruccion.Dominio;

namespace Eventos.Dominio.EventosDominio;

public class EventoPublicadoEventoDominio : EventoDominio
{
    public Guid EventoId { get; }
    public string TituloEvento { get; }
    public DateTime FechaInicio { get; }
    public bool EsVirtual { get; }

    public EventoPublicadoEventoDominio(Guid eventoId, string tituloEvento, DateTime fechaInicio, bool esVirtual)
    {
        EventoId = eventoId;
        TituloEvento = tituloEvento;
        FechaInicio = fechaInicio;
        EsVirtual = esVirtual;
    }
}
