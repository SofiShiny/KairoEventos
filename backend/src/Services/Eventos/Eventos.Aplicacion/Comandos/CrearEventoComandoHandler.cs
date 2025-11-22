using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Aplicacion.DTOs;
using FluentValidation;
using MediatR;

namespace Eventos.Aplicacion.Comandos;

public class CrearEventoComandoHandler : IRequestHandler<CrearEventoComando, Resultado<EventoDto>>
{
    private readonly IRepositorioEvento _repositorioEvento;
    private readonly IValidator<CrearEventoComando> _validator;

    public CrearEventoComandoHandler(IRepositorioEvento repositorioEvento, IValidator<CrearEventoComando> validator)
    {
        _repositorioEvento = repositorioEvento;
        _validator = validator;
    }

    public async Task<Resultado<EventoDto>> Handle(CrearEventoComando request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);
        if (!validation.IsValid)
            return Resultado<EventoDto>.Falla(validation.Errors.First().ErrorMessage);

        var evento = CrearEvento(request);

        await _repositorioEvento.AgregarAsync(evento, cancellationToken);

        return Resultado<EventoDto>.Exito(EventoDtoMapper.Map(evento));
    }

    private static Evento CrearEvento(CrearEventoComando request)
    {
        var ubicacion = MapUbicacion(request.Ubicacion!);
        return new Evento(
            request.Titulo,
            request.Descripcion,
            ubicacion,
            request.FechaInicio,
            request.FechaFin,
            request.MaximoAsistentes,
            request.OrganizadorId);
    }

    private static Ubicacion MapUbicacion(UbicacionDto dto) => new(
        dto.NombreLugar!,
        dto.Direccion!,
        dto.Ciudad!,
        dto.Region ?? string.Empty,
        dto.CodigoPostal ?? string.Empty,
        dto.Pais!);
}
