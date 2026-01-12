using MediatR;

namespace BloquesConstruccion.Aplicacion.Comandos;

public interface IComando : IRequest
{
}

public interface IComando<out TResultado> : IRequest<TResultado>
{
}
