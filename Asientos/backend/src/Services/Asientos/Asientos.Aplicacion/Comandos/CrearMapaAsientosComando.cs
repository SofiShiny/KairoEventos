using MediatR;

namespace Asientos.Aplicacion.Comandos;

public record CrearMapaAsientosComando(Guid EventoId) : IRequest<Guid>;
