using MediatR;

namespace Asientos.Aplicacion.Comandos;

public record AgregarAsientoComando(Guid MapaId, int Fila, int Numero, string Categoria) : IRequest<Guid>;
