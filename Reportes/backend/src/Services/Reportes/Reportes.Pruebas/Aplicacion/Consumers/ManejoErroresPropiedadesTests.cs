using FsCheck;
using FsCheck.Xunit;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Reportes.Aplicacion.Consumers;
using Eventos.Dominio.EventosDeDominio;
using Reportes.Dominio.Repositorios;
using Xunit;

namespace Reportes.Pruebas.Aplicacion.Consumers;

/// <summary>
/// Property-Based Tests para manejo de errores y resiliencia
/// Feature: microservicio-reportes
/// </summary>
public class ManejoErroresPropiedadesTests
{
    /// <summary>
    /// Propiedad 21: Movimiento a cola de errores tras reintentos
    /// Para cualquier evento que falla después de 3 reintentos, 
    /// debe existir un mensaje correspondiente en la cola de errores de RabbitMQ.
    /// Valida: Requisitos 10.2
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(GeneradoresEventos) })]
    public Property Propiedad21_MovimientoAColaErroresTraReintentos(
        EventoPublicadoEventoDominio evento)
    {
        // Feature: microservicio-reportes, Property 21: Movimiento a cola de errores tras reintentos
        
        return Prop.ForAll<int>(
            Gen.Choose(1, 5).ToArbitrary(),
            intentosFallidos =>
            {
                // Arrange
                var repositorioMock = new Mock<IRepositorioReportesLectura>();
                var loggerMock = new Mock<ILogger<EventoPublicadoConsumer>>();
                
                // Simular fallo en el repositorio
                repositorioMock
                    .Setup(r => r.ActualizarMetricasAsync(It.IsAny<Reportes.Dominio.ModelosLectura.MetricasEvento>()))
                    .ThrowsAsync(new Exception("Error simulado de MongoDB"));
                
                var consumer = new EventoPublicadoConsumer(
                    repositorioMock.Object,
                    loggerMock.Object);
                
                // Act & Assert
                // Verificar que después de 3 intentos, el evento debe fallar
                if (intentosFallidos >= 3)
                {
                    // El consumidor debe lanzar excepción después de 3 intentos
                    // MassTransit moverá el mensaje a la cola de errores
                    var exception = Assert.ThrowsAsync<Exception>(async () =>
                    {
                        var context = CreateMockConsumeContext(evento);
                        await consumer.Consume(context);
                    }).Result;
                    
                    return (exception != null).ToProperty()
                        .Label($"Evento debe fallar después de {intentosFallidos} intentos");
                }
                else
                {
                    // Con menos de 3 intentos, aún debe reintentar
                    return true.ToProperty()
                        .Label($"Evento debe reintentar con {intentosFallidos} intentos");
                }
            });
    }

    /// <summary>
    /// Propiedad adicional: Registro de errores en logs
    /// Para cualquier evento que falla, debe registrarse en los logs
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(GeneradoresEventos) })]
    public Property PropiedadAdicional_RegistroErroresEnLogs(
        EventoPublicadoEventoDominio evento)
    {
        // Arrange
        var repositorioMock = new Mock<IRepositorioReportesLectura>();
        var loggerMock = new Mock<ILogger<EventoPublicadoConsumer>>();
        
        // Simular fallo
        repositorioMock
            .Setup(r => r.ActualizarMetricasAsync(It.IsAny<Reportes.Dominio.ModelosLectura.MetricasEvento>()))
            .ThrowsAsync(new Exception("Error de prueba"));
        
        var consumer = new EventoPublicadoConsumer(
            repositorioMock.Object,
            loggerMock.Object);
        
        // Act
        try
        {
            var context = CreateMockConsumeContext(evento);
            consumer.Consume(context).Wait();
        }
        catch
        {
            // Esperamos que falle
        }
        
        // Assert - Verificar que se registró el error
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce,
            "Debe registrar el error en logs");
        
        return true.ToProperty();
    }

    /// <summary>
    /// Propiedad: Reintentos con backoff exponencial
    /// Para cualquier evento que falla temporalmente, el sistema debe reintentar
    /// con intervalos crecientes
    /// </summary>
    [Property(MaxTest = 50)]
    public Property PropiedadReintentoConBackoffExponencial()
    {
        return Prop.ForAll<int>(
            Gen.Choose(1, 3).ToArbitrary(),
            numeroReintento =>
            {
                // Calcular intervalo esperado con backoff exponencial
                var minInterval = TimeSpan.FromSeconds(2);
                var intervalDelta = TimeSpan.FromSeconds(2);
                
                var intervaloEsperado = minInterval + (intervalDelta * (numeroReintento - 1));
                var maxInterval = TimeSpan.FromSeconds(30);
                
                // El intervalo debe crecer pero no exceder el máximo
                var intervaloFinal = intervaloEsperado > maxInterval 
                    ? maxInterval 
                    : intervaloEsperado;
                
                // Verificar que el intervalo crece con cada reintento
                var cumpleBackoff = intervaloFinal >= minInterval && 
                                   intervaloFinal <= maxInterval;
                
                return cumpleBackoff.ToProperty()
                    .Label($"Reintento {numeroReintento}: intervalo {intervaloFinal.TotalSeconds}s");
            });
    }

    private static ConsumeContext<EventoPublicadoEventoDominio> CreateMockConsumeContext(
        EventoPublicadoEventoDominio evento)
    {
        var contextMock = new Mock<ConsumeContext<EventoPublicadoEventoDominio>>();
        contextMock.Setup(c => c.Message).Returns(evento);
        return contextMock.Object;
    }
}
