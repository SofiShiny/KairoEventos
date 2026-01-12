using Entradas.Infraestructura.Servicios;
using Entradas.Dominio.Interfaces;

namespace Entradas.Pruebas.Infraestructura;

/// <summary>
/// Clase de demostración para mostrar el funcionamiento del GeneradorCodigoQr
/// </summary>
public class GeneradorCodigoQrDemostracion
{
    [Fact]
    public void DemostrarGeneracionCodigosQr()
    {
        // Arrange
        IGeneradorCodigoQr generador = new GeneradorCodigoQr();
        
        // Act & Assert - Generar y mostrar algunos códigos de ejemplo
        var codigosGenerados = new List<string>();
        
        for (int i = 0; i < 5; i++)
        {
            var codigo = generador.GenerarCodigoUnico();
            codigosGenerados.Add(codigo);
            
            // Verificar que cada código tiene el formato correcto
            Assert.True(generador.ValidarFormato(codigo), $"El código {codigo} debe tener formato válido");
            Assert.Matches(@"^TICKET-[A-F0-9]{8}-\d{4}$", codigo);
        }
        
        // Verificar que todos los códigos son únicos
        Assert.Equal(5, codigosGenerados.Distinct().Count());
        
        // Los códigos generados se pueden ver en la salida de test si se ejecuta con verbosidad
        foreach (var codigo in codigosGenerados)
        {
            Assert.NotNull(codigo);
            Assert.StartsWith("TICKET-", codigo);
        }
    }
}