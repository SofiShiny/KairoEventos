using BloquesConstruccion.Dominio;

namespace Asientos.Dominio.EventosDominio;

public class AsientoAgregadoEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public int Fila { get; }
    public int Numero { get; }
    public string Categoria { get; }
    
    public AsientoAgregadoEventoDominio(Guid mapaId, int fila, int numero, string categoria)
    {
        MapaId = mapaId;
        Fila = fila;
        Numero = numero;
        Categoria = categoria;
        IdAgregado = mapaId;
    }
}
