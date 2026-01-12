using Pagos.Dominio.Entidades;

namespace Pagos.Dominio.Interfaces;

public record ResultadoPago(bool Exitoso, string? MotivoRechazo, string? TransaccionIdExterno);

public interface IPasarelaPago
{
    Task<ResultadoPago> CobrarAsync(decimal monto, string tarjeta);
    Task<IEnumerable<dynamic>> ObtenerMovimientosAsync(); // Para conciliación
}

public interface IGeneradorFactura
{
    byte[] GenerarPdf(Transaccion tx);
}

public interface IAlmacenadorArchivos
{
    Task<string> GuardarAsync(string nombre, byte[] datos);
}

public interface IRepositorioTransacciones
{
    Task<Transaccion?> ObtenerPorIdAsync(Guid id);
    Task AgregarAsync(Transaccion tx);
    Task ActualizarAsync(Transaccion tx);
    Task<IEnumerable<Transaccion>> ObtenerTodasAsync();
}
