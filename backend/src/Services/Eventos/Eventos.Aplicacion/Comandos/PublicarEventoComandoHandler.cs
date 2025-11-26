using BloquesConstruccion.Aplicacion.Comandos;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using MediatR;

namespace Eventos.Aplicacion.Comandos;

public class PublicarEventoComandoHandler : IRequestHandler<PublicarEventoComando, Resultado>
{
    private readonly IRepositorioEvento _repositorioEvento;

    public PublicarEventoComandoHandler(IRepositorioEvento repositorioEvento)
    {
        _repositorioEvento = repositorioEvento;
    }

    public async Task<Resultado> Handle(PublicarEventoComando request, CancellationToken cancellationToken)
    {
        var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, cancellationToken);
        
        if (evento == null)
            return Resultado.Falla("Evento no encontrado");

        try
        {
            evento.Publicar();
            await _repositorioEvento.ActualizarAsync(evento, cancellationToken);
            return Resultado.Exito();
        }
        catch (InvalidOperationException ex)
        {
            return Resultado.Falla(ex.Message);
        }
    }
}
