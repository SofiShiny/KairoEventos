using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using MediatR;
using Moq;
using FluentAssertions;
using Entradas.API.Controllers;
using Entradas.API.DTOs;
using Entradas.Aplicacion.Comandos;
using Entradas.Aplicacion.Queries;
using Entradas.Aplicacion.DTOs;
using Entradas.Dominio.Excepciones;
using Entradas.Dominio.Enums;

namespace Entradas.Pruebas.API.Controllers;

/// <summary>
/// Pruebas unitarias para el controlador EntradasController
/// Valida el comportamiento de todos los endpoints y manejo de excepciones
/// </summary>
public class EntradasControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<EntradasController>> _loggerMock;
    private readonly EntradasController _controller;

    public EntradasControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<EntradasController>>();
        _controller = new EntradasController(_mediatorMock.Object, _loggerMock.Object);
        
        // Setup HttpContext for controller
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.HttpContext.Request.Path = "/api/entradas";
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ConParametrosValidos_DebeCrearInstancia()
    {
        // Arrange & Act
        var controller = new EntradasController(_mediatorMock.Object, _loggerMock.Object);

        // Assert
        controller.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConMediatorNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new EntradasController(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("mediator");
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var act = () => new EntradasController(_mediatorMock.Object, null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("logger");
    }

    #endregion

    #region CrearEntrada Tests

    [Fact]
    public async Task CrearEntrada_ConDatosValidos_DebeRetornarCreatedResult()
    {
        // Arrange
        var request = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            AsientoId = Guid.NewGuid()
        };

        var entradaCreada = new EntradaCreadaDto(
            Id: Guid.NewGuid(),
            EventoId: request.EventoId,
            UsuarioId: request.UsuarioId,
            AsientoId: request.AsientoId,
            Monto: 100.50m,
            CodigoQr: "TICKET-ABC123-4567",
            Estado: EstadoEntrada.PendientePago,
            FechaCompra: DateTime.UtcNow
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<CrearEntradaCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(entradaCreada);

        // Act
        var result = await _controller.CrearEntrada(request);

        // Assert
        result.Should().NotBeNull();
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(EntradasController.ObtenerEntrada));
        createdResult.Value.Should().BeEquivalentTo(entradaCreada);

        // Verify mediator was called with correct command
        _mediatorMock.Verify(m => m.Send(
            It.Is<CrearEntradaCommand>(c => 
                c.EventoId == request.EventoId &&
                c.UsuarioId == request.UsuarioId &&
                c.AsientoId == request.AsientoId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearEntrada_ConEventoNoDisponible_DebeRetornarNotFound()
    {
        // Arrange
        var request = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid()
        };

        var exception = new EventoNoDisponibleException(request.EventoId, "El evento no está disponible");
        _mediatorMock.Setup(m => m.Send(It.IsAny<CrearEntradaCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(exception);

        // Act
        var result = await _controller.CrearEntrada(request);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
        
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Evento no disponible");
        problemDetails.Detail.Should().Be(exception.Message);
        problemDetails.Status.Should().Be(404);
    }

    [Fact]
    public async Task CrearEntrada_ConAsientoNoDisponible_DebeRetornarConflict()
    {
        // Arrange
        var request = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            AsientoId = Guid.NewGuid()
        };

        var exception = new AsientoNoDisponibleException("El asiento no está disponible");
        _mediatorMock.Setup(m => m.Send(It.IsAny<CrearEntradaCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(exception);

        // Act
        var result = await _controller.CrearEntrada(request);

        // Assert
        result.Should().NotBeNull();
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);
        
        var problemDetails = conflictResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Asiento no disponible");
        problemDetails.Detail.Should().Be(exception.Message);
        problemDetails.Status.Should().Be(409);
    }

    [Fact]
    public async Task CrearEntrada_ConExcepcionDeDominio_DebeRetornarBadRequest()
    {
        // Arrange
        var request = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid()
        };

        var exception = new EstadoEntradaInvalidoException("Estado de entrada inválido");
        _mediatorMock.Setup(m => m.Send(It.IsAny<CrearEntradaCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(exception);

        // Act
        var result = await _controller.CrearEntrada(request);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        
        var problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Error de validación");
        problemDetails.Detail.Should().Be(exception.Message);
        problemDetails.Status.Should().Be(400);
    }

    [Fact]
    public async Task CrearEntrada_SinAsientoId_DebeCrearEntradaGeneral()
    {
        // Arrange
        var request = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            AsientoId = null // Entrada general
        };

        var entradaCreada = new EntradaCreadaDto(
            Id: Guid.NewGuid(),
            EventoId: request.EventoId,
            UsuarioId: request.UsuarioId,
            AsientoId: null,
            Monto: 75.00m,
            CodigoQr: "TICKET-DEF456-7890",
            Estado: EstadoEntrada.PendientePago,
            FechaCompra: DateTime.UtcNow
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<CrearEntradaCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(entradaCreada);

        // Act
        var result = await _controller.CrearEntrada(request);

        // Assert
        result.Should().NotBeNull();
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        
        var entrada = createdResult.Value.Should().BeOfType<EntradaCreadaDto>().Subject;
        entrada.AsientoId.Should().BeNull();
    }

    #endregion

    #region ObtenerEntrada Tests

    [Fact]
    public async Task ObtenerEntrada_ConIdExistente_DebeRetornarOkResult()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var entradaDto = new EntradaDto(
            Id: entradaId,
            EventoId: Guid.NewGuid(),
            UsuarioId: Guid.NewGuid(),
            AsientoId: Guid.NewGuid(),
            Monto: 150.00m,
            CodigoQr: "TICKET-GHI789-0123",
            Estado: EstadoEntrada.Pagada,
            FechaCompra: DateTime.UtcNow.AddDays(-1),
            EventoNombre: "Evento",
            AsientoInfo: "General",
            EsVirtual: false
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<ObtenerEntradaQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(entradaDto);

        // Act
        var result = await _controller.ObtenerEntrada(entradaId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(entradaDto);

        // Verify mediator was called with correct query
        _mediatorMock.Verify(m => m.Send(
            It.Is<ObtenerEntradaQuery>(q => q.EntradaId == entradaId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerEntrada_ConIdNoExistente_DebeRetornarNotFound()
    {
        // Arrange
        var entradaId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<ObtenerEntradaQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((EntradaDto?)null);

        // Act
        var result = await _controller.ObtenerEntrada(entradaId);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
        
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Entrada no encontrada");
        problemDetails.Detail.Should().Contain(entradaId.ToString());
        problemDetails.Status.Should().Be(404);
    }

    [Fact]
    public async Task ObtenerEntrada_ConExcepcionDeDominio_DebeRetornarBadRequest()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var exception = new EstadoEntradaInvalidoException("Error de validación en consulta");

        _mediatorMock.Setup(m => m.Send(It.IsAny<ObtenerEntradaQuery>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(exception);

        // Act
        var result = await _controller.ObtenerEntrada(entradaId);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        
        var problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Error de validación");
        problemDetails.Detail.Should().Be(exception.Message);
        problemDetails.Status.Should().Be(400);
    }

    #endregion

    #region ObtenerEntradasPorUsuario Tests

    [Fact]
    public async Task ObtenerEntradasPorUsuario_ConUsuarioConEntradas_DebeRetornarListaDeEntradas()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var entradas = new List<EntradaDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), usuarioId, Guid.NewGuid(), 100m, "TICKET-001", EstadoEntrada.Pagada, DateTime.UtcNow.AddDays(-2), "Evento", "General", false),
            new(Guid.NewGuid(), Guid.NewGuid(), usuarioId, null, 75m, "TICKET-002", EstadoEntrada.PendientePago, DateTime.UtcNow.AddDays(-1), "Evento", "General", false)
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ObtenerEntradasPorUsuarioQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(entradas);

        // Act
        var result = await _controller.ObtenerEntradasPorUsuario(usuarioId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedEntradas = okResult.Value.Should().BeAssignableTo<IEnumerable<EntradaDto>>().Subject;
        returnedEntradas.Should().HaveCount(2);
        returnedEntradas.Should().BeEquivalentTo(entradas);

        // Verify mediator was called with correct query
        _mediatorMock.Verify(m => m.Send(
            It.Is<ObtenerEntradasPorUsuarioQuery>(q => q.UsuarioId == usuarioId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerEntradasPorUsuario_ConUsuarioSinEntradas_DebeRetornarListaVacia()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var entradasVacias = new List<EntradaDto>();

        _mediatorMock.Setup(m => m.Send(It.IsAny<ObtenerEntradasPorUsuarioQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(entradasVacias);

        // Act
        var result = await _controller.ObtenerEntradasPorUsuario(usuarioId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedEntradas = okResult.Value.Should().BeAssignableTo<IEnumerable<EntradaDto>>().Subject;
        returnedEntradas.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerEntradasPorUsuario_ConExcepcionDeDominio_DebeRetornarBadRequest()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var exception = new EstadoEntradaInvalidoException("Usuario inválido");

        _mediatorMock.Setup(m => m.Send(It.IsAny<ObtenerEntradasPorUsuarioQuery>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(exception);

        // Act
        var result = await _controller.ObtenerEntradasPorUsuario(usuarioId);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        
        var problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Error de validación");
        problemDetails.Detail.Should().Be(exception.Message);
        problemDetails.Status.Should().Be(400);
    }

    #endregion

    #region Health Tests

    [Fact]
    public void Health_DebeRetornarOkConEstadoHealthy()
    {
        // Act
        var result = _controller.Health();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var healthStatus = okResult.Value;
        healthStatus.Should().NotBeNull();
        
        // Verify the response has the expected structure (anonymous type with status and timestamp)
        var healthStatusString = healthStatus!.ToString();
        healthStatusString.Should().Contain("status");
        healthStatusString.Should().Contain("timestamp");
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task CrearEntrada_DebeLoguearInformacionDeInicio()
    {
        // Arrange
        var request = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid()
        };

        var entradaCreada = new EntradaCreadaDto(
            Guid.NewGuid(), request.EventoId, request.UsuarioId, null, 100m,
            "TICKET-TEST", EstadoEntrada.PendientePago, DateTime.UtcNow
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<CrearEntradaCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(entradaCreada);

        // Act
        await _controller.CrearEntrada(request);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando creación de entrada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Entrada creada exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CrearEntrada_ConEventoNoDisponible_DebeLoguearWarning()
    {
        // Arrange
        var request = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid()
        };

        var exception = new EventoNoDisponibleException(request.EventoId, "Evento no disponible");
        _mediatorMock.Setup(m => m.Send(It.IsAny<CrearEntradaCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(exception);

        // Act
        await _controller.CrearEntrada(request);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Evento no disponible")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerEntrada_ConIdNoExistente_DebeLoguearWarning()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<ObtenerEntradaQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((EntradaDto?)null);

        // Act
        await _controller.ObtenerEntrada(entradaId);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Entrada no encontrada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Edge Cases and Validation Tests

    [Fact]
    public async Task CrearEntrada_ConCancellationToken_DebeRespetarCancelacion()
    {
        // Arrange
        var request = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid()
        };

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _mediatorMock.Setup(m => m.Send(It.IsAny<CrearEntradaCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _controller.CrearEntrada(request, cancellationTokenSource.Token));
    }

    #endregion
}