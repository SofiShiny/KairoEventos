using BloquesConstruccion.Dominio;

namespace Asientos.Dominio.EventosDominio;

public class AsientoLiberadoEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public Guid AsientoId { get; }
    public int Fila { get; }
    public int Numero { get; }
    
    public AsientoLiberadoEventoDominio(Guid mapaId, Guid asientoId, int fila, int numero)
    {
        MapaId = mapaId;
        AsientoId = asientoId;
        Fila = fila;
        Numero = numero;
        IdAgregado = mapaId;
    }
}
