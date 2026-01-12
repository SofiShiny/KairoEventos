using BloquesConstruccion.Aplicacion.Comandos;
using BloquesConstruccion.Aplicacion.Comun;
using MediatR;

namespace Eventos.Aplicacion.Comandos;

public record CancelarEventoComando(Guid EventoId) : IRequest<Resultado>;
