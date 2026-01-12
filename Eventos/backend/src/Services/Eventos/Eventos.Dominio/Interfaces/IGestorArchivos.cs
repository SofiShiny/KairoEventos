using System.IO;
using System.Threading.Tasks;

namespace Eventos.Dominio.Interfaces;

public interface IGestorArchivos
{
    Task<string> SubirImagenAsync(Stream archivo, string nombre, string carpeta);
    Task BorrarImagenAsync(string rutaRelativa);
}
