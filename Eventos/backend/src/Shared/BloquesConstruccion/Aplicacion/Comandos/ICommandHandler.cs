using MediatR;

namespace BloquesConstruccion.Aplicacion.Comandos;

public interface IComandoHandler<in TComando> : IRequestHandler<TComando>
 where TComando : IComando
{
}

public interface IComandoHandler<in TComando, TResult> : IRequestHandler<TComando, TResult>
 where TComando : IComando<TResult>
{
}
