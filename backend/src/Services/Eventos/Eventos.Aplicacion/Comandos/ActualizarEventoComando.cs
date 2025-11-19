using BloquesConstruccion.Aplicacion.Comun;
using BloquesConstruccion.Aplicacion.Comandos;
using Eventos.Aplicacion.DTOs;

namespace Eventos.Aplicacion.Comandos;

public record ActualizarEventoComando(
 Guid EventoId,
 string? Titulo,
 string? Descripcion,
 UbicacionDto? Ubicacion,
 DateTime? FechaInicio,
 DateTime? FechaFin,
 int? MaximoAsistentes
) : IComando<Resultado<EventoDto>>;