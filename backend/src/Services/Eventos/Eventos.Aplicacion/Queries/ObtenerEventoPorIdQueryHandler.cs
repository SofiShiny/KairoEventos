using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using Eventos.Aplicacion.DTOs;
using MediatR;

namespace Eventos.Aplicacion.Queries;

public class ObtenerEventoPorIdQueryHandler : IRequestHandler<ObtenerEventoPorIdQuery, Resultado<EventoDto>>
{
    private readonly IRepositorioEvento _repositorioEvento;

    public ObtenerEventoPorIdQueryHandler(IRepositorioEvento repositorioEvento)
    {
        _repositorioEvento = repositorioEvento;
    }

    public async Task<Resultado<EventoDto>> Handle(ObtenerEventoPorIdQuery request, CancellationToken cancellationToken)
    {
        var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, cancellationToken);
        
        if (evento == null)
            return Resultado<EventoDto>.Falla("Evento no encontrado");

        if (evento.Ubicacion == null)
            return Resultado<EventoDto>.Falla("Los datos de ubicación del evento no son válidos");

        var dto = new EventoDto
        {
            Id = evento.Id,
            Titulo = evento.Titulo,
            Descripcion = evento.Descripcion,
            Ubicacion = new UbicacionDto
            {
                NombreLugar = evento.Ubicacion.NombreLugar,
                Direccion = evento.Ubicacion.Direccion,
                Ciudad = evento.Ubicacion.Ciudad,
                Region = evento.Ubicacion.Region,
                CodigoPostal = evento.Ubicacion.CodigoPostal,
                Pais = evento.Ubicacion.Pais
            },
            FechaInicio = evento.FechaInicio,
            FechaFin = evento.FechaFin,
            MaximoAsistentes = evento.MaximoAsistentes,
            ConteoAsistentesActual = evento.ConteoAsistentesActual,
            Estado = evento.Estado.ToString(),
            OrganizadorId = evento.OrganizadorId,
            CreadoEn = evento.CreadoEn,
            Asistentes = evento.Asistentes.Select(a => new AsistenteDto
            {
                Id = a.Id,
                NombreUsuario = a.NombreUsuario,
                Correo = a.Correo,
                RegistradoEn = a.RegistradoEn
            }).ToList()
        };

        return Resultado<EventoDto>.Exito(dto);
    }
}
