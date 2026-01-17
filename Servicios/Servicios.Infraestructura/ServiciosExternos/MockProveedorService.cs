using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Servicios.Dominio.Interfaces;

namespace Servicios.Infraestructura.ServiciosExternos;

public class MockProveedorService : IProveedorExternoService
{
    private readonly Random _random = new();

    public async Task<IEnumerable<ServicioExternoDto>> ObtenerServiciosPorTipoAsync(string tipo)
    {
        await Task.Delay(100); 

        if (tipo.ToLower().Contains("transporte"))
        {
            return new List<ServicioExternoDto>
            {
                new() { IdServicioExterno = "1", Nombre = "Bus (Mock)", Tipo = "transporte", Precio = 10.5m, Disponible = true },
                new() { IdServicioExterno = "2", Nombre = "Taxi (Mock)", Tipo = "transporte", Precio = 20.0m, Disponible = true }
            };
        }

        return Enumerable.Empty<ServicioExternoDto>();
    }

    public async Task<IEnumerable<ServicioExternoDto>> ObtenerTodosLosServiciosAsync()
    {
        return new List<ServicioExternoDto>
        {
            new() { IdServicioExterno = "1", Nombre = "Bus (Mock)", Tipo = "transporte", Precio = 10.5m, Disponible = true },
            new() { IdServicioExterno = "2", Nombre = "Taxi (Mock)", Tipo = "transporte", Precio = 20.0m, Disponible = true }
        };
    }

    public Task ActualizarServicioAsync(string idExterno, decimal precio, bool disponible)
    {
        return Task.CompletedTask;
    }
}
