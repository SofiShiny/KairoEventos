using Pagos.Dominio.Interfaces;

namespace Pagos.Infraestructura.Pasarela;

public class SimuladorPasarela : IPasarelaPago
{
    public async Task<ResultadoPago> CobrarAsync(decimal monto, string tarjeta)
    {
        await Task.Delay(1000); // Simular latencia

        if (tarjeta.EndsWith("5555") || tarjeta.EndsWith("5000"))
        {
            // Simular error de red (Hangfire reintentará)
            throw new HttpRequestException("Error de comunicación temporal con la pasarela (Simulado)");
        }

        if (tarjeta.EndsWith("4444"))
        {
            // Simular rechazo permanente de la pasarela
            return new ResultadoPago(false, "Pago fallido no procesado: fondos insuficientes o tarjeta rechazada", null);
        }

        return new ResultadoPago(true, null, Guid.NewGuid().ToString());
    }

    public Task<IEnumerable<dynamic>> ObtenerMovimientosAsync()
    {
        return Task.FromResult(Enumerable.Empty<dynamic>());
    }
}
