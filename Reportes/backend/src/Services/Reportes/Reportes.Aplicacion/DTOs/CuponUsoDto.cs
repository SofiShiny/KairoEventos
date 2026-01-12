namespace Reportes.Aplicacion.DTOs;

/// <summary>
/// DTO que representa el uso de un cupón específico
/// </summary>
public class CuponUsoDto
{
    public string Codigo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}
