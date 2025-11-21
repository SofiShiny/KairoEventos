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
        var validacion = Validar(request);
        if (validacion.EsFallido) return Resultado<EventoDto>.Falla(validacion.Error);

        try
        {
            var ubicacion = MapUbicacion(request.Ubicacion!);
            var evento = new Evento(request.Titulo, request.Descripcion, ubicacion, request.FechaInicio, request.FechaFin, request.MaximoAsistentes, request.OrganizadorId);
            await _repositorioEvento.AgregarAsync(evento, cancellationToken);
            return Resultado<EventoDto>.Exito(MapEvento(evento));
        }
        catch (ArgumentException ex)
        {
            return Resultado<EventoDto>.Falla(ex.Message);
        }
    }

    private Resultado Validar(CrearEventoComando request)
    {
        if (request.Ubicacion is null) return Resultado.Falla("La ubicacion es obligatoria");
        if (request.FechaFin <= request.FechaInicio) return Resultado.Falla("La fecha fin debe ser posterior a la fecha inicio");
        if (request.MaximoAsistentes <= 0) return Resultado.Falla("El maximo de asistentes debe ser mayor que cero");
        return Resultado.Exito();
    }

    private Ubicacion MapUbicacion(UbicacionDto dto) => new(
        dto.NombreLugar ?? string.Empty,
        dto.Direccion ?? string.Empty,
        dto.Ciudad ?? string.Empty,
        dto.Region ?? string.Empty,
        dto.CodigoPostal ?? string.Empty,
        dto.Pais ?? string.Empty
    );

    private static UbicacionDto MapUbicacionDto(Ubicacion u) => new()
    {
        NombreLugar = u.NombreLugar,
        Direccion = u.Direccion,
        Ciudad = u.Ciudad,
        Region = u.Region,
        CodigoPostal = u.CodigoPostal,
        Pais = u.Pais
    };

    private EventoDto MapEvento(Evento e)
    {
        // Dominio garantiza que Ubicacion nunca sea null
        var ubicacionDto = MapUbicacionDto(e.Ubicacion);
        return new EventoDto
        {
            Id = e.Id,
            Titulo = e.Titulo,
            Descripcion = e.Descripcion,
            Ubicacion = ubicacionDto,
            FechaInicio = e.FechaInicio,
            FechaFin = e.FechaFin,
            MaximoAsistentes = e.MaximoAsistentes,
            ConteoAsistentesActual = e.ConteoAsistentesActual,
            Estado = e.Estado.ToString(),
            OrganizadorId = e.OrganizadorId,
            CreadoEn = e.CreadoEn
        };
    }
}
