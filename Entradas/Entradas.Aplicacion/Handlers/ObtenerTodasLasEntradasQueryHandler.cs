using MediatR;
using Entradas.Aplicacion.DTOs;
using Entradas.Aplicacion.Mappers;
using Entradas.Dominio.Interfaces;
using Entradas.Aplicacion.Queries;

namespace Entradas.Aplicacion.Handlers;

public class ObtenerTodasLasEntradasQueryHandler : IRequestHandler<ObtenerTodasLasEntradasQuery, IEnumerable<EntradaDto>>
{
    private readonly IRepositorioEntradas _repositorio;
    private readonly IVerificadorEventos _verificadorEventos;

    public ObtenerTodasLasEntradasQueryHandler(
        IRepositorioEntradas repositorio,
        IVerificadorEventos verificadorEventos)
    {
        _repositorio = repositorio;
        _verificadorEventos = verificadorEventos;
    }

    public async Task<IEnumerable<EntradaDto>> Handle(
        ObtenerTodasLasEntradasQuery request, 
        CancellationToken cancellationToken)
    {
        var entradas = await _repositorio.ObtenerTodasAsync(cancellationToken);

        // Si hay un filtro por organizador, necesitamos validar qué eventos le pertenecen
        if (request.OrganizadorId.HasValue)
        {
            var todasEntradas = entradas.ToList();
            var eventIdsUnicos = todasEntradas.Select(e => e.EventoId).Distinct().ToList();
            var eventosDelOrganizador = new HashSet<Guid>();

            foreach (var eventId in eventIdsUnicos)
            {
                var eventoInfo = await _verificadorEventos.ObtenerInfoEventoAsync(eventId, cancellationToken);
                if (eventoInfo != null && eventoInfo.OrganizadorId == request.OrganizadorId.Value.ToString())
                {
                    eventosDelOrganizador.Add(eventId);
                }
            }

            entradas = todasEntradas.Where(e => eventosDelOrganizador.Contains(e.EventoId));
        }

        return EntradaMapper.ToDto(entradas);
    }
}
