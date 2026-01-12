using BloquesConstruccion.Dominio;

namespace Asientos.Dominio.EventosDominio;

public class AsientoReservadoEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public int Fila { get; }
    public int Numero { get; }
    
    public AsientoReservadoEventoDominio(Guid mapaId, int fila, int numero)
    {
        MapaId = mapaId;
        Fila = fila;
        Numero = numero;
        IdAgregado = mapaId;
    }
}
