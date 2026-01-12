namespace Reportes.API.DTOs;

public class PaginacionDto<T>
{
    public List<T> Datos { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TamañoPagina { get; set; }
    public long TotalRegistros { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamañoPagina);
    public bool TienePaginaAnterior => PaginaActual > 1;
    public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
}
