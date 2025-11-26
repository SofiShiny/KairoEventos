using BloquesConstruccion.Aplicacion.Queries;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using Eventos.Aplicacion.DTOs;
using MediatR;
using AutoMapper;

namespace Eventos.Aplicacion.Queries;

public class ObtenerEventoPorIdQueryHandler : IRequestHandler<ObtenerEventoPorIdQuery, Resultado<EventoDto>>
{
    private readonly IRepositorioEvento _repositorioEvento;
    private readonly IMapper _mapper;
    private const string ErrorNoEncontrado = "Evento no encontrado";
    private const string ErrorUbicacionInvalida = "Los datos de ubicacion del evento no son validos";

    public ObtenerEventoPorIdQueryHandler(IRepositorioEvento repositorioEvento, IMapper mapper)
    {
        _repositorioEvento = repositorioEvento;
        _mapper = mapper;
    }

    public async Task<Resultado<EventoDto>> Handle(ObtenerEventoPorIdQuery request, CancellationToken cancellationToken)
    {
        var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, cancellationToken);
        if (evento is null) return Resultado<EventoDto>.Falla(ErrorNoEncontrado);
        if (evento.Ubicacion is null) return Resultado<EventoDto>.Falla(ErrorUbicacionInvalida);
        return Resultado<EventoDto>.Exito(_mapper.Map<EventoDto>(evento));
    }
}
