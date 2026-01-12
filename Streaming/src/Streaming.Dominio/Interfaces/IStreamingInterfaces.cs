using Streaming.Dominio.Entidades;

namespace Streaming.Dominio.Interfaces;

public interface IRepositorioTransmisiones
{
    Task<Transmision?> ObtenerPorEventoIdAsync(Guid eventoId);
    Task AgregarAsync(Transmision transmision);
}

public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
