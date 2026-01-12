namespace BloquesConstruccion.Infraestructura.Persistencia;

public interface IUnidadDeTrabajo : IDisposable
{
    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
    Task IniciarTransaccionAsync(CancellationToken cancellationToken = default);
    Task ConfirmarTransaccionAsync(CancellationToken cancellationToken = default);
    Task RevertirTransaccionAsync(CancellationToken cancellationToken = default);
}
