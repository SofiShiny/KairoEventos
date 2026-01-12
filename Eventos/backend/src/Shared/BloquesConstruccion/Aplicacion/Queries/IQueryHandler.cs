using MediatR;

namespace BloquesConstruccion.Aplicacion.Queries;

public interface IQueryHandler<in TQuery, TResultado> : IRequestHandler<TQuery, TResultado>
    where TQuery : IQuery<TResultado>
{
}
