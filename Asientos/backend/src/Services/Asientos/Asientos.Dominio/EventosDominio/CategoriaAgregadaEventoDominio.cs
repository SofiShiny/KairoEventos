using BloquesConstruccion.Dominio;

namespace Asientos.Dominio.EventosDominio;

public class CategoriaAgregadaEventoDominio : EventoDominio
{
    public Guid MapaId { get; }
    public string NombreCategoria { get; }
    
    public CategoriaAgregadaEventoDominio(Guid mapaId, string nombreCategoria)
    {
        MapaId = mapaId;
        NombreCategoria = nombreCategoria;
        IdAgregado = mapaId;
    }
}
