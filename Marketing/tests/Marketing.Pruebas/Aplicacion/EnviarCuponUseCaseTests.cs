using Moq;
using Marketing.Aplicacion.CasosUso;
using Marketing.Aplicacion.Interfaces;
using Marketing.Dominio.Entidades;
using Marketing.Dominio.Enums;
using Marketing.Dominio.Eventos;
using FluentAssertions;
using Xunit;

namespace Marketing.Pruebas.Aplicacion;

public class EnviarCuponUseCaseTests
{
    private readonly Mock<IRepositorioCupones> _repositorioMock;
    private readonly Mock<IEventoPublicador> _publicadorMock;
    private readonly EnviarCuponUseCase _useCase;

    public EnviarCuponUseCaseTests()
    {
        _repositorioMock = new Mock<IRepositorioCupones>();
        _publicadorMock = new Mock<IEventoPublicador>();
        _useCase = new EnviarCuponUseCase(_repositorioMock.Object, _publicadorMock.Object);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoCuponEsDisponible_DebeAsignarYPublicarEvento()
    {
        // Arrange
        var codigo = "DESC10";
        var usuarioId = Guid.NewGuid();
        var cupon = new Cupon(codigo, TipoDescuento.Porcentaje, 10, DateTime.UtcNow.AddDays(1));
        
        _repositorioMock.Setup(r => r.ObtenerPorCodigoAsync(codigo))
            .ReturnsAsync(cupon);

        // Act
        await _useCase.EjecutarAsync(codigo, usuarioId);

        // Assert
        _repositorioMock.Verify(r => r.ActualizarAsync(It.Is<Cupon>(c => c.UsuarioDestinatarioId == usuarioId)), Times.Once);
        _publicadorMock.Verify(p => p.PublicarAsync(It.IsAny<CuponEnviadoEvento>()), Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_CuandoCuponYaEstaUsado_DebeLanzarExcepcion()
    {
        // Arrange
        var codigo = "USADO";
        var cupon = new Cupon(codigo, TipoDescuento.Porcentaje, 10, DateTime.UtcNow.AddDays(1));
        cupon.MarcarComoUsado(Guid.NewGuid());

        _repositorioMock.Setup(r => r.ObtenerPorCodigoAsync(codigo))
            .ReturnsAsync(cupon);

        // Act
        Func<Task> act = async () => await _useCase.EjecutarAsync(codigo, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Solo se pueden enviar cupones disponibles.");
    }
}
