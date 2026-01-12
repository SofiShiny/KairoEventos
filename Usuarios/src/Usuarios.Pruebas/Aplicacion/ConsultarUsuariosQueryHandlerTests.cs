using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Usuarios.Aplicacion.Consultas;
using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.Enums;
using Usuarios.Dominio.ObjetosValor;
using Usuarios.Dominio.Repositorios;

namespace Usuarios.Pruebas.Aplicacion;

public class ConsultarUsuariosQueryHandlerTests
{
    private readonly Mock<IRepositorioUsuarios> _mockRepositorio;
    private readonly Mock<ILogger<ConsultarUsuariosQueryHandler>> _mockLogger;
    private readonly ConsultarUsuariosQueryHandler _handler;

    public ConsultarUsuariosQueryHandlerTests()
    {
        _mockRepositorio = new Mock<IRepositorioUsuarios>();
        _mockLogger = new Mock<ILogger<ConsultarUsuariosQueryHandler>>();
        
        _handler = new ConsultarUsuariosQueryHandler(
            _mockRepositorio.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_RetornaListaDeUsuarioDtos()
    {
        // Arrange
        var usuarios = new List<Usuario>
        {
            Usuario.Crear(
                "user1",
                "User One",
                Correo.Crear("user1@example.com"),
                Telefono.Crear("1234567890"),
                Direccion.Crear("Calle 1"),
                Rol.User),
            Usuario.Crear(
                "user2",
                "User Two",
                Correo.Crear("user2@example.com"),
                Telefono.Crear("9876543210"),
                Direccion.Crear("Calle 2"),
                Rol.Admin)
        };

        var query = new ConsultarUsuariosQuery();

        _mockRepositorio
            .Setup(r => r.ObtenerActivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarios);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.First().Username.Should().Be("user1");
        resultado.Last().Username.Should().Be("user2");
    }

    [Fact]
    public async Task Handle_SoloRetornaUsuariosActivos()
    {
        // Arrange
        var usuarioActivo = Usuario.Crear(
            "activeuser",
            "Active User",
            Correo.Crear("active@example.com"),
            Telefono.Crear("1234567890"),
            Direccion.Crear("Calle Activa"),
            Rol.User);

        var usuarios = new List<Usuario> { usuarioActivo };

        var query = new ConsultarUsuariosQuery();

        _mockRepositorio
            .Setup(r => r.ObtenerActivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarios);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(1);
        resultado.First().Username.Should().Be("activeuser");
        
        // Verificar que se llamó al método correcto del repositorio
        _mockRepositorio.Verify(
            r => r.ObtenerActivosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConListaVacia_RetornaListaVacia()
    {
        // Arrange
        var usuarios = new List<Usuario>();

        var query = new ConsultarUsuariosQuery();

        _mockRepositorio
            .Setup(r => r.ObtenerActivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarios);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }
}
