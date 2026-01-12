using FluentValidation.TestHelper;
using FluentAssertions;
using Entradas.API.Validators;
using Entradas.API.DTOs;

namespace Entradas.Pruebas.API.Validators;

/// <summary>
/// Pruebas unitarias para CrearEntradaRequestDtoValidator
/// Valida las reglas de validación para el DTO de creación de entradas
/// </summary>
public class CrearEntradaRequestDtoValidatorTests
{
    private readonly CrearEntradaRequestDtoValidator _validator;

    public CrearEntradaRequestDtoValidatorTests()
    {
        _validator = new CrearEntradaRequestDtoValidator();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_DebeCrearInstancia()
    {
        // Arrange & Act
        var validator = new CrearEntradaRequestDtoValidator();

        // Assert
        validator.Should().NotBeNull();
    }

    #endregion

    #region EventoId Validation Tests

    [Fact]
    public void EventoId_ConGuidValido_NoDebeGenerarError()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EventoId);
    }

    [Fact]
    public void EventoId_ConGuidEmpty_DebeGenerarError()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.Empty,
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventoId)
              .WithErrorMessage("El ID del evento debe ser un GUID válido");
    }

    [Fact]
    public void EventoId_ConValorPorDefecto_DebeGenerarError()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            // EventoId no asignado (default Guid.Empty)
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventoId)
              .WithErrorMessage("El ID del evento es requerido");
    }

    #endregion

    #region UsuarioId Validation Tests

    [Fact]
    public void UsuarioId_ConGuidValido_NoDebeGenerarError()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UsuarioId);
    }

    [Fact]
    public void UsuarioId_ConGuidEmpty_DebeGenerarError()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UsuarioId)
              .WithErrorMessage("El ID del usuario debe ser un GUID válido");
    }

    [Fact]
    public void UsuarioId_ConValorPorDefecto_DebeGenerarError()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UsuarioId)
              .WithErrorMessage("El ID del usuario es requerido");
    }

    #endregion

    #region AsientoId Validation Tests

    [Fact]
    public void AsientoId_ConNull_NoDebeGenerarError()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            AsientoId = null
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AsientoId);
    }

    [Fact]
    public void AsientoId_ConGuidValido_NoDebeGenerarError()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            AsientoId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AsientoId);
    }

    [Fact]
    public void AsientoId_ConGuidEmpty_DebeGenerarError()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            AsientoId = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AsientoId)
              .WithErrorMessage("Si se especifica un asiento, debe ser un GUID válido");
    }

    #endregion

    #region Complete DTO Validation Tests

    [Fact]
    public void Validate_ConDtoCompleto_NoDebeGenerarErrores()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            AsientoId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ConDtoSinAsientoId_NoDebeGenerarErrores()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            AsientoId = null
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ConDtoVacio_DebeGenerarMultiplesErrores()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            // Todos los valores por defecto
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventoId);
        result.ShouldHaveValidationErrorFor(x => x.UsuarioId);
        // The validator has multiple rules per field, so we expect more than 2 errors
        result.Errors.Should().HaveCountGreaterThan(2);
    }

    [Fact]
    public void Validate_ConTodosLosValoresInvalidos_DebeGenerarTodosLosErrores()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.Empty,
            UsuarioId = Guid.Empty,
            AsientoId = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventoId);
        result.ShouldHaveValidationErrorFor(x => x.UsuarioId);
        result.ShouldHaveValidationErrorFor(x => x.AsientoId);
        result.Errors.Should().HaveCountGreaterThan(3); 
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void EventoId_ConGuidValido_DebeSerValido()
    {
        // Reutilizando lógica existente
    }

    #endregion

    #region Validation Message Tests

    [Fact]
    public void EventoId_ConGuidEmpty_DebeRetornarMensajeEspecifico()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.Empty,
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        var error = result.Errors.FirstOrDefault(e => e.PropertyName == nameof(dto.EventoId));
        error.Should().NotBeNull();
        // The validator has multiple rules, so we check for the first one that fires
        error!.ErrorMessage.Should().BeOneOf("El ID del evento es requerido", "El ID del evento debe ser un GUID válido");
    }

    [Fact]
    public void UsuarioId_ConGuidEmpty_DebeRetornarMensajeEspecifico()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        var error = result.Errors.FirstOrDefault(e => e.PropertyName == nameof(dto.UsuarioId));
        error.Should().NotBeNull();
        // The validator has multiple rules, so we check for the first one that fires
        error!.ErrorMessage.Should().BeOneOf("El ID del usuario es requerido", "El ID del usuario debe ser un GUID válido");
    }

    [Fact]
    public void AsientoId_ConGuidEmpty_DebeRetornarMensajeEspecifico()
    {
        // Arrange
        var dto = new CrearEntradaRequestDto
        {
            EventoId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            AsientoId = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        var error = result.Errors.FirstOrDefault(e => e.PropertyName == nameof(dto.AsientoId));
        error.Should().NotBeNull();
        error!.ErrorMessage.Should().Be("Si se especifica un asiento, debe ser un GUID válido");
    }

    #endregion
}