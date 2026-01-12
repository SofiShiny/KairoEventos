using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Servicios.Dominio.Interfaces;

namespace Servicios.Infraestructura.ServiciosExternos;

public class MockProveedorService : IProveedorExternoService
{
    private readonly Random _random = new();

    public async Task<Dictionary<string, bool>> ConsultarEstadoProveedoresAsync(List<string> externalIds)
    {
        // Simular latencia de red
        await Task.Delay(200);

        var resultados = new Dictionary<string, bool>();

        foreach (var id in externalIds)
        {
            // Lógica por defecto: disponible
            bool disponible = true;

            // Lógica especial para Ridery: falla el 30% de las veces
            if (id.Contains("RIDERY", StringComparison.OrdinalIgnoreCase))
            {
                disponible = _random.NextDouble() > 0.3;
            }

            resultados.Add(id, disponible);
        }

        return resultados;
    }
}
