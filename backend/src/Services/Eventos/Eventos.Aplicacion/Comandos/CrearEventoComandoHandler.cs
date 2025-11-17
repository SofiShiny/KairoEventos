using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Aplicacion.DTOs;
using MediatR;

namespace Eventos.Aplicacion.Comandos;

public class CrearEventoComandoHandler : IRequestHandler<CrearEventoComando, Resultado<EventoDto>>
{
    private readonly IRepositorioEvento _repositorioEvento;

    public CrearEventoComandoHandler(IRepositorioEvento repositorioEvento)
    {
        _repositorioEvento = repositorioEvento;
    }

    public async Task<Resultado<EventoDto>> Handle(CrearEventoComando request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Ubicacion == null)
            {
                return Resultado<EventoDto>.Falla("La ubicación es obligatoria");
            }

            var ubicacion = new Ubicacion(
                request.Ubicacion.NombreLugar ?? string.Empty,
                request.Ubicacion.Direccion ?? string.Empty,
                request.Ubicacion.Ciudad ?? string.Empty,
                request.Ubicacion.Region ?? string.Empty,
                request.Ubicacion.CodigoPostal ?? string.Empty,
                request.Ubicacion.Pais ?? string.Empty
            );

            var evento = new Evento(
                request.Titulo,
                request.Descripcion,
                ubicacion,
                request.FechaInicio,
                request.FechaFin,
                request.MaximoAsistentes,
                request.OrganizadorId
            );

            await _repositorioEvento.AgregarAsync(evento, cancellationToken);

            var dto = MapToDto(evento);
            return Resultado<EventoDto>.Exito(dto);
        }
        catch (ArgumentException ex)
        {
            return Resultado<EventoDto>.Falla(ex.Message);
        }
    }

    private EventoDto MapToDto(Evento evento)
    {
        return new EventoDto
        {
            Id = evento.Id,
            Titulo = evento.Titulo,
            Descripcion = evento.Descripcion,
            Ubicacion = new UbicacionDto
            {
                NombreLugar = evento.Ubicacion?.NombreLugar ?? string.Empty,
                Direccion = evento.Ubicacion?.Direccion ?? string.Empty,
                Ciudad = evento.Ubicacion?.Ciudad ?? string.Empty,
                Region = evento.Ubicacion?.Region ?? string.Empty,
                CodigoPostal = evento.Ubicacion?.CodigoPostal ?? string.Empty,
                Pais = evento.Ubicacion?.Pais ?? string.Empty
            },
            FechaInicio = evento.FechaInicio,
            FechaFin = evento.FechaFin,
            MaximoAsistentes = evento.MaximoAsistentes,
            ConteoAsistentesActual = evento.ConteoAsistentesActual,
            Estado = evento.Estado.ToString(),
            OrganizadorId = evento.OrganizadorId,
            CreadoEn = evento.CreadoEn
        };
    }
}
