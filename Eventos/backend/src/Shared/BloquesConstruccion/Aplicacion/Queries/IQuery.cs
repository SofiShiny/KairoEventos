using MediatR;

namespace BloquesConstruccion.Aplicacion.Queries;

public interface IQuery<out TResultado> : IRequest<TResultado>
{
}
