using Pagos.Dominio.Interfaces;

namespace Pagos.Infraestructura.Almacenamiento;

public class AlmacenadorLocal : IAlmacenadorArchivos
{
    public async Task<string> GuardarAsync(string nombre, byte[] datos)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "facturas");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        var fullPath = Path.Combine(path, nombre);
        await File.WriteAllBytesAsync(fullPath, datos);

        return $"/facturas/{nombre}";
    }
}
