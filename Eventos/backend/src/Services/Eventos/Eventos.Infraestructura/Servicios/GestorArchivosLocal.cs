using System;
using System.IO;
using System.Threading.Tasks;
using Eventos.Dominio.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Eventos.Infraestructura.Servicios;

public class GestorArchivosLocal : IGestorArchivos
{
    private readonly IWebHostEnvironment _env;

    public GestorArchivosLocal(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SubirImagenAsync(Stream archivo, string nombre, string carpeta)
    {
        var wwwrootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var targetFolder = Path.Combine(wwwrootPath, carpeta);
        
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        var extension = Path.GetExtension(nombre);
        var nombreUnico = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(targetFolder, nombreUnico);

        using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            await archivo.CopyToAsync(fileStream);
        }

        return $"/{carpeta}/{nombreUnico}";
    }

    public Task BorrarImagenAsync(string rutaRelativa)
    {
        if (string.IsNullOrEmpty(rutaRelativa))
            return Task.CompletedTask;

        var wwwrootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var fullPath = Path.Combine(wwwrootPath, rutaRelativa.TrimStart('/'));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
