using Asientos.Dominio.Agregados;
using Asientos.Dominio.Entidades;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Asientos.Dominio.Repositorios;

public interface IRepositorioMapaAsientos
{
    Task<MapaAsientos?> ObtenerPorIdAsync(Guid id, CancellationToken ct);
    Task AgregarAsync(MapaAsientos mapa, CancellationToken ct);
    Task ActualizarAsync(MapaAsientos mapa, CancellationToken ct);
    Task<Guid> AgregarAsientoAsync(MapaAsientos mapa, Asiento asiento, CancellationToken ct);
    Task<Asiento?> ObtenerAsientoPorIdAsync(Guid asientoId, CancellationToken ct);
}
