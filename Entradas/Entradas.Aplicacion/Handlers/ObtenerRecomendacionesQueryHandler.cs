using MediatR;
using Microsoft.Extensions.Logging;
using Entradas.Aplicacion.Queries;
using Entradas.Aplicacion.DTOs;
using Entradas.Dominio.Interfaces;
using Entradas.Dominio.Entidades;

namespace Entradas.Aplicacion.Handlers;

public class ObtenerRecomendacionesQueryHandler : IRequestHandler<ObtenerRecomendacionesQuery, IEnumerable<EventoRecomendadoDto>>
{
    private readonly IRepositorioEntradas _repositorioEntradas;
    private readonly IVerificadorEventos _verificadorEventos;
    private readonly ILogger<ObtenerRecomendacionesQueryHandler> _logger;

    public ObtenerRecomendacionesQueryHandler(
        IRepositorioEntradas repositorioEntradas,
        IVerificadorEventos verificadorEventos,
        ILogger<ObtenerRecomendacionesQueryHandler> logger)
    {
        _repositorioEntradas = repositorioEntradas;
        _verificadorEventos = verificadorEventos;
        _logger = logger;
    }

    public async Task<IEnumerable<EventoRecomendadoDto>> Handle(ObtenerRecomendacionesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo recomendaciones para usuario {UsuarioId}", request.UsuarioId);

        // 1. Obtener todas las entradas del usuario
        var entradas = await _repositorioEntradas.ObtenerPorUsuarioAsync(request.UsuarioId, cancellationToken);
        
        // 2. Determinar la categoría de interés (basada en la última compra con categoría válida)
        string? categoriaInteres = entradas
            .OrderByDescending(e => e.FechaCompra)
            .Select(e => e.Categoria)
            .FirstOrDefault(c => !string.IsNullOrWhiteSpace(c));

        if (string.IsNullOrWhiteSpace(categoriaInteres))
        {
            _logger.LogInformation("Usuario {UsuarioId} no tiene historial de categorías. Consultando eventos generales.", request.UsuarioId);
        }
        else
        {
            _logger.LogInformation("Categoría de interés detectada: {Categoria}", categoriaInteres);
        }

        // 3. Consultar sugerencias al microservicio de Eventos
        var sugerencias = await _verificadorEventos.ObtenerSugerenciasAsync(categoriaInteres, request.Cantidad, cancellationToken);

        // 4. Mapear a DTO
        return sugerencias.Select(s => new EventoRecomendadoDto(
            s.Id,
            s.Titulo,
            s.Categoria,
            s.FechaInicio,
            s.UrlImagen
        ));
    }
}
