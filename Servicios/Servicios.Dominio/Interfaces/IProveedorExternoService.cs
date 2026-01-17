using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicios.Dominio.Interfaces;

public interface IProveedorExternoService
{
    Task<IEnumerable<ServicioExternoDto>> ObtenerServiciosPorTipoAsync(string tipo);
    Task<IEnumerable<ServicioExternoDto>> ObtenerTodosLosServiciosAsync();
    Task ActualizarServicioAsync(string idExterno, decimal precio, bool disponible);
}

public class ServicioExternoDto
{
    public string IdServicioExterno { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public bool Disponible { get; set; }
}
