using System;
using System.Threading;
using System.Threading.Tasks;

namespace Entradas.Dominio.Interfaces;

public record AsientoDto(
    Guid Id,
    string Nombre,
    decimal Precio,
    string Seccion,
    int Fila,
    int Numero,
    bool EstaDisponible
);

public interface IAsientosService
{
    Task<AsientoDto> GetAsientoByIdAsync(Guid asientoId, CancellationToken cancellationToken = default);
}
