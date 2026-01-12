using System.ComponentModel.DataAnnotations;

namespace Eventos.Aplicacion.DTOs;

public class UbicacionDto
{
    [Required] public string? NombreLugar { get; set; }
    [Required] public string? Direccion { get; set; }
    [Required] public string? Ciudad { get; set; }
    public string? Region { get; set; }
    public string? CodigoPostal { get; set; }
    [Required] public string? Pais { get; set; }
}
