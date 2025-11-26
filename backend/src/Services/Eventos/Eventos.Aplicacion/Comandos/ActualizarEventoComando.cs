using BloquesConstruccion.Aplicacion.Comun;
using BloquesConstruccion.Aplicacion.Comandos;
using Eventos.Aplicacion.DTOs;

namespace Eventos.Aplicacion.Comandos;
// Se usa ? en los parametros porque se pueden actualizar solo uno o dos campos
//El handler decidira que campos actualizar
public record ActualizarEventoComando(
 Guid EventoId,
 string? Titulo,
 string? Descripcion,
 UbicacionDto? Ubicacion,
 DateTime? FechaInicio,
 DateTime? FechaFin,
 int? MaximoAsistentes
) : IComando<Resultado<EventoDto>>;