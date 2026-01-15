using MediatR;
using Recomendaciones.Aplicacion.DTOs;

namespace Recomendaciones.Aplicacion.Queries;

/// <summary>
/// Query para obtener los eventos más populares (tendencias)
/// </summary>
public record ObtenerTendenciasQuery(int Limite = 5) : IRequest<List<EventoRecomendadoDto>>;

/// <summary>
/// Query para obtener recomendaciones personalizadas para un usuario
/// </summary>
public record ObtenerRecomendacionesPersonalizadasQuery(Guid UsuarioId) : IRequest<RecomendacionesPersonalizadasDto>;
