using System;
using System.Threading;
using System.Threading.Tasks;
using Eventos.Dominio.Interfaces;
using Eventos.Dominio.Repositorios;
using MediatR;
using BloquesConstruccion.Aplicacion.Comun;

namespace Eventos.Aplicacion.Comandos;

public class ActualizarImagenEventoComandoHandler : IRequestHandler<ActualizarImagenEventoComando, Resultado<string>>
{
    private readonly IRepositorioEvento _repositorio;
    private readonly IGestorArchivos _gestorArchivos;

    public ActualizarImagenEventoComandoHandler(IRepositorioEvento repositorio, IGestorArchivos gestorArchivos)
    {
        _repositorio = repositorio;
        _gestorArchivos = gestorArchivos;
    }

    public async Task<Resultado<string>> Handle(ActualizarImagenEventoComando request, CancellationToken cancellationToken)
    {
        try 
        {
            var evento = await _repositorio.ObtenerPorIdAsync(request.EventoId, cancellationToken: cancellationToken);
            
            if (evento == null)
            {
                return Resultado<string>.Falla($"No se encontró el evento con ID {request.EventoId}");
            }

            // Borrar imagen previa si existe
            if (!string.IsNullOrEmpty(evento.UrlImagen))
            {
                await _gestorArchivos.BorrarImagenAsync(evento.UrlImagen);
            }

            // Subir nueva imagen
            var urlImagen = await _gestorArchivos.SubirImagenAsync(request.Archivo, request.NombreArchivo, "imagenes");

            // Actualizar entidad
            evento.ActualizarImagen(urlImagen);

            // Guardar cambios
            await _repositorio.ActualizarAsync(evento, cancellationToken);

            return Resultado<string>.Exito(urlImagen);
        }
        catch (Exception ex)
        {
            return Resultado<string>.Falla($"Error al actualizar imagen: {ex.Message}");
        }
    }
}
