using BloquesConstruccion.Aplicacion.Comandos;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using MediatR;

namespace Eventos.Aplicacion.Comandos;

public class RegistrarAsistenteComandoHandler : IRequestHandler<RegistrarAsistenteComando, Resultado>
{
    private readonly IRepositorioEvento _repositorioEvento;

    public RegistrarAsistenteComandoHandler(IRepositorioEvento repositorioEvento)
    {
        _repositorioEvento = repositorioEvento;
    }

    public async Task<Resultado> Handle(RegistrarAsistenteComando request, CancellationToken cancellationToken)
    {
        var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, cancellationToken);
        
        if (evento == null)
            return Resultado.Falla("Evento no encontrado");

        try
        {
            evento.RegistrarAsistente(request.UsuarioId, request.NombreUsuario, request.Correo);
            await _repositorioEvento.ActualizarAsync(evento, cancellationToken);
            return Resultado.Exito();
        }
        catch (InvalidOperationException ex)
        {
            return Resultado.Falla(ex.Message);
        }
    }
}
