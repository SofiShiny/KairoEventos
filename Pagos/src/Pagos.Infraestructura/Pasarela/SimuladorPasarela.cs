using Pagos.Dominio.Interfaces;

namespace Pagos.Infraestructura.Pasarela;

public class SimuladorPasarela : IPasarelaPago
{
    public async Task<ResultadoPago> CobrarAsync(decimal monto, string tarjeta)
    {
        await Task.Delay(1000); // Simular latencia

        if (tarjeta.EndsWith("5000"))
        {
            throw new HttpRequestException("Error de comunicación temporal con la pasarela (Simulado)");
        }

        return new ResultadoPago(true, null, Guid.NewGuid().ToString());
    }

    public Task<IEnumerable<dynamic>> ObtenerMovimientosAsync()
    {
        return Task.FromResult(Enumerable.Empty<dynamic>());
    }
}
