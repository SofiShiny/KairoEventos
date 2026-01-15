using BloquesConstruccion.Aplicacion.Comun;
using MediatR;

namespace Eventos.Aplicacion.Comandos;

public record FinalizarEventoComando(Guid EventoId) : IRequest<Resultado>;
