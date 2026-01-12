using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Usuarios.API.Controllers;
using Usuarios.Aplicacion.Comandos;
using Usuarios.Aplicacion.Consultas;
using Usuarios.Aplicacion.DTOs;
using Usuarios.Dominio.Enums;

namespace Usuarios.Pruebas.API;

public class UsuariosControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<UsuariosController>> _loggerMock;
    private readonly UsuariosController _controller;

    public UsuariosControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<UsuariosController>>();
        _controller = new UsuariosController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Crear_ConDatosValidos_Retorna201CreatedConGuid()
    {
        // Arrange
        var dto = new CrearUsuarioDto
        {
            Username = "testuser",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Test Address 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        var expectedId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AgregarUsuarioComando>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _controller.Crear(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.StatusCode.Should().Be(201);
        createdResult.Value.Should().Be(expectedId);
        createdResult.ActionName.Should().Be(nameof(UsuariosController.ObtenerPorId));
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues!["id"].Should().Be(expectedId);

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<AgregarUsuarioComando>(c =>
                    c.Username == dto.Username &&
                    c.Nombre == dto.Nombre &&
                    c.Correo == dto.Correo &&
                    c.Telefono == dto.Telefono &&
                    c.Direccion == dto.Direccion &&
                    c.Rol == dto.Rol &&
                    c.Password == dto.Password),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerPorId_ConUsuarioExistente_Retorna200OKConUsuarioDto()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var expectedDto = new UsuarioDto
        {
            Id = usuarioId,
            Username = "testuser",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Test Address 123",
            Rol = Rol.User,
            FechaCreacion = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ConsultarUsuarioQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.ObtenerPorId(usuarioId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedDto);

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<ConsultarUsuarioQuery>(q => q.UsuarioId == usuarioId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerPorId_ConUsuarioInexistente_Retorna404NotFound()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ConsultarUsuarioQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioDto?)null);

        // Act
        var result = await _controller.ObtenerPorId(usuarioId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = result as NotFoundResult;
        notFoundResult!.StatusCode.Should().Be(404);

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<ConsultarUsuarioQuery>(q => q.UsuarioId == usuarioId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerTodos_Retorna200OKConListaDeUsuarios()
    {
        // Arrange
        var expectedUsuarios = new List<UsuarioDto>
        {
            new UsuarioDto
            {
                Id = Guid.NewGuid(),
                Username = "user1",
                Nombre = "User One",
                Correo = "user1@example.com",
                Telefono = "1234567890",
                Direccion = "Address 1",
                Rol = Rol.User,
                FechaCreacion = DateTime.UtcNow
            },
            new UsuarioDto
            {
                Id = Guid.NewGuid(),
                Username = "user2",
                Nombre = "User Two",
                Correo = "user2@example.com",
                Telefono = "0987654321",
                Direccion = "Address 2",
                Rol = Rol.Admin,
                FechaCreacion = DateTime.UtcNow
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ConsultarUsuariosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUsuarios);

        // Act
        var result = await _controller.ObtenerTodos(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedUsuarios);

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<ConsultarUsuariosQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Actualizar_ConDatosValidos_Retorna204NoContent()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dto = new ActualizarUsuarioDto
        {
            Nombre = "Updated Name",
            Telefono = "9876543210",
            Direccion = "Updated Address 456"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ActualizarUsuarioComando>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.Actualizar(usuarioId, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult!.StatusCode.Should().Be(204);

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<ActualizarUsuarioComando>(c =>
                    c.UsuarioId == usuarioId &&
                    c.Nombre == dto.Nombre &&
                    c.Telefono == dto.Telefono &&
                    c.Direccion == dto.Direccion),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Eliminar_ConUsuarioExistente_Retorna204NoContent()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<EliminarUsuarioComando>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.Eliminar(usuarioId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult!.StatusCode.Should().Be(204);

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<EliminarUsuarioComando>(c => c.UsuarioId == usuarioId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
