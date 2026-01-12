using BloquesConstruccion.Dominio;
using System.Linq;
using Xunit;

namespace Asientos.Pruebas.Dominio.BloquesConstruccion;

// Clases concretas para probar la clase abstracta RaizAgregada
file class EventoPrueba : EventoDominio { }
file class RaizAgregadaConcreta : RaizAgregada
{
    public void HacerAlgo()
    {
        GenerarEventoDominio(new EventoPrueba());
    }
}

public class RaizAgregadaTests
{
    [Fact]
    public void GenerarEventoDominio_DebeAgregarEventoALaColeccion()
    {
        // Arrange
        var raiz = new RaizAgregadaConcreta();

        // Act
        raiz.HacerAlgo();

        // Assert
        Assert.Single(raiz.Eventos);
        Assert.IsType<EventoPrueba>(raiz.Eventos.First());
    }
}
