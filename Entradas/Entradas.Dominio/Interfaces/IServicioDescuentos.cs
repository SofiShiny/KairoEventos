using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Entradas.Dominio.Interfaces;

public record ResultadoDescuento(
    bool Aplicado,
    decimal MontoDescuento,
    string Mensaje);

public interface IServicioDescuentos
{
    Task<ResultadoDescuento> CalcularDescuentoAsync(Guid usuarioId, List<string> cupones, decimal montoOriginal);
}
