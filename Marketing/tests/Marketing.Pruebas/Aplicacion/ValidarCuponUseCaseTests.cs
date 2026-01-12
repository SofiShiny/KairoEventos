using Moq;
using Marketing.Aplicacion.CasosUso;
using Marketing.Aplicacion.Interfaces;
using Marketing.Dominio.Entidades;
using Marketing.Dominio.Enums;
using FluentAssertions;
using Xunit;

namespace Marketing.Pruebas.Aplicacion;

public class ValidarCuponUseCaseTests
{
    private readonly Mock<IRepositorioCupones> _repositorioMock;
    private readonly ValidarCuponUseCase _useCase;

    public ValidarCuponUseCaseTests()
    {
        _repositorioMock = new Mock<IRepositorioCupones>();
        _useCase = new ValidarCuponUseCase(_repositorioMock.Object);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoCuponEstaExpirado_DebeRetornarInvalido()
    {
        // Arrange
        var codigo = "EXPIRADO";
        // Simulamos un cupón que expiró ayer (necesitamos usar reflexión o un hack de tiempo si la entidad valida en constructor)
        // En este caso, la entidad valida expiracion > UtcNow en el constructor.
        // Para testear esto, podemos crear uno válido y esperar, o cambiar el diseño para inyectar un proveedor de tiempo.
        // Como no tenemos proveedor de tiempo, usaremos un código que no pase EsValido() por fecha.
        
        // Pero espera, el constructor tira excepcion si le paso fecha pasada.
        // OPCION A: Agregar un constructor interno/propiedad para tests.
        // OPCION B: Inyectar IDateTimeProvider (Mejor arquitectura).
        
        // Vamos a inyectar ICurrentTimeProvider en la entidad o pasarle el tiempo como parametro.
        // Por ahora, para resolver rápido, asumiremos que el repositorio nos devuelve un objeto ya expirado.
        
        // NOTA: Para que esto funcione, EF Core puede cargar objetos que el constructor no dejaría crear.
        // Usaremos Moq para devolver un objeto con estado de expirado si fuera posible.
        // Como las propiedades son private set, usaremos un objeto real pero el constructor nos bloquea.
        
        // FIX: Cambiaré el constructor de la entidad o usaré uno específico para tests.
        // Decisión: Inyectaré un tiempo simulado en el test si es posible via Moq o simplemente crearé el test para "Disponible".
        
        // Mejor: Implementaré ValidarCuponUseCase para que maneje la lógica de tiempo comparando con la entidad.
        
        var cuponInvalido = new Cupon("VALIDO", TipoDescuento.MontoFijo, 50, DateTime.UtcNow.AddDays(1));
        cuponInvalido.ForceExpiracion(DateTime.UtcNow.AddDays(-1)); // Ayer

        _repositorioMock.Setup(r => r.ObtenerPorCodigoAsync("VALIDO"))
            .ReturnsAsync(cuponInvalido);

        // Act
        var resultado = await _useCase.EjecutarAsync("VALIDO");

        // Assert
        resultado.EsValido.Should().BeFalse();
        resultado.Mensaje.Should().Contain("expirado");
    }
}
