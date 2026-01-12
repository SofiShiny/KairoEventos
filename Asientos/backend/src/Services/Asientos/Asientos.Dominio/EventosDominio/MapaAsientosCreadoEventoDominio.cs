using BloquesConstruccion.Dominio;

namespace Asientos.Dominio.EventosDominio;

public class MapaAsientosCreadoEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public Guid EventoId { get; }
    
    public MapaAsientosCreadoEventoDominio(Guid mapaId, Guid eventoId)
    {
        MapaId = mapaId;
        EventoId = eventoId;
        IdAgregado = mapaId;
    }
}
