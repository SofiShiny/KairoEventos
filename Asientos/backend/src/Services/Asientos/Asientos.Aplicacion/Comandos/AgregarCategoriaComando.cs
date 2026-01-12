using MediatR;

namespace Asientos.Aplicacion.Comandos;

public record AgregarCategoriaComando(Guid MapaId, string Nombre, decimal? PrecioBase, bool TienePrioridad) : IRequest<Guid>;
