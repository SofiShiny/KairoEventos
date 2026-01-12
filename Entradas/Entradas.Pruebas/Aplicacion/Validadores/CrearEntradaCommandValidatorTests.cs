using FluentAssertions;
using FluentValidation.TestHelper;
using Entradas.Aplicacion.Comandos;
using Entradas.Aplicacion.Validadores;

namespace Entradas.Pruebas.Aplicacion.Validadores;

/// <summary>
/// Pruebas comprehensivas para CrearEntradaCommandValidator
/// Cubre todas las validaciones de negocio
/// </summary>
public class CrearEntradaCommandValidatorTests
{
    private readonly CrearEntradaCommandValidator _validator;

    public CrearEntradaCommandValidatorTests()
    {
        _validator = new CrearEntradaCommandValidator();
    }

    [Fact]
    public void Validate_ConDatosValidos_DebeSerValido()
    {
        // Arrange
        var command = new CrearEntradaCommand(
            EventoId: Guid.NewGuid(),
            UsuarioId: Guid.NewGuid(),
            AsientoId: Guid.NewGuid()
        );

        // Act
        var resultado = _validator.TestValidate(command);

        // Assert
        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ConEntradaGeneral_DebeSerValido()
    {
        // Arrange
        var command = new CrearEntradaCommand(
            EventoId: Guid.NewGuid(),
            UsuarioId: Guid.NewGuid(),
            AsientoId: null // Entrada general
        );

        // Act
        var resultado = _validator.TestValidate(command);

        // Assert
        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Validate_ConEventoIdVacio_DebeSerInvalido(string guidString)
    {
        // Arrange
        var eventoId = Guid.Parse(guidString);
        var command = new CrearEntradaCommand(
            EventoId: eventoId,
            UsuarioId: Guid.NewGuid(),
            AsientoId: null
        );

        // Act
        var resultado = _validator.TestValidate(command);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.EventoId)
            .WithErrorMessage("El ID del evento debe ser un GUID válido");
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Validate_ConUsuarioIdVacio_DebeSerInvalido(string guidString)
    {
        // Arrange
        var usuarioId = Guid.Parse(guidString);
        var command = new CrearEntradaCommand(
            EventoId: Guid.NewGuid(),
            UsuarioId: usuarioId,
            AsientoId: null
        );

        // Act
        var resultado = _validator.TestValidate(command);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.UsuarioId)
            .WithErrorMessage("El ID del usuario debe ser un GUID válido");
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Validate_ConAsientoIdVacio_DebeSerInvalido(string guidString)
    {
        // Arrange
        var asientoId = Guid.Parse(guidString);
        var command = new CrearEntradaCommand(
            EventoId: Guid.NewGuid(),
            UsuarioId: Guid.NewGuid(),
            AsientoId: asientoId
        );

        // Act
        var resultado = _validator.TestValidate(command);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.AsientoId)
            .WithErrorMessage("Si se especifica un asiento, debe ser un GUID válido");
    }

    [Fact]
    public void Validate_ConTodosLosCamposInvalidos_DebeRetornarTodosLosErrores()
    {
        // Arrange
        var command = new CrearEntradaCommand(
            EventoId: Guid.Empty,
            UsuarioId: Guid.Empty,
            AsientoId: Guid.Empty
        );

        // Act
        var resultado = _validator.TestValidate(command);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.EventoId);
        resultado.ShouldHaveValidationErrorFor(x => x.UsuarioId);
        resultado.ShouldHaveValidationErrorFor(x => x.AsientoId);
        resultado.ShouldHaveValidationErrorFor(x => x.AsientoId);
    }

    [Fact]
    public void Validate_ConAsientoIdNulo_NoDebeValidarAsiento()
    {
        // Arrange
        var command = new CrearEntradaCommand(
            EventoId: Guid.NewGuid(),
            UsuarioId: Guid.NewGuid(),
            AsientoId: null
        );

        // Act
        var resultado = _validator.TestValidate(command);

        // Assert
        resultado.ShouldNotHaveValidationErrorFor(x => x.AsientoId);
    }

    [Fact]
    public void Validate_ConAsientoIdValido_DebeSerValido()
    {
        // Arrange
        var command = new CrearEntradaCommand(
            EventoId: Guid.NewGuid(),
            UsuarioId: Guid.NewGuid(),
            AsientoId: Guid.NewGuid()
        );

        // Act
        var resultado = _validator.TestValidate(command);

        // Assert
        resultado.ShouldNotHaveValidationErrorFor(x => x.AsientoId);
    }

    [Fact]
    public void Validate_ConGuidsDiferentes_DebeSerValido()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();

        // Asegurar que todos los GUIDs sean diferentes
        eventoId.Should().NotBe(usuarioId);
        eventoId.Should().NotBe(asientoId);
        usuarioId.Should().NotBe(asientoId);

        var command = new CrearEntradaCommand(
            EventoId: eventoId,
            UsuarioId: usuarioId,
            AsientoId: asientoId
        );

        // Act
        var resultado = _validator.TestValidate(command);

        // Assert
        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ConGuidsIguales_DebeSerValido()
    {
        // Arrange - Aunque no es un caso de uso real, técnicamente es válido
        var guid = Guid.NewGuid();
        var command = new CrearEntradaCommand(
            EventoId: guid,
            UsuarioId: guid, // Mismo GUID (técnicamente válido)
            AsientoId: guid
        );

        // Act
        var resultado = _validator.TestValidate(command);

        // Assert
        resultado.ShouldNotHaveAnyValidationErrors();
    }
}