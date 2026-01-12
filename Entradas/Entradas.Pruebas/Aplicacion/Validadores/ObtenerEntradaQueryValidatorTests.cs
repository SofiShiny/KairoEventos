using FluentAssertions;
using FluentValidation.TestHelper;
using Entradas.Aplicacion.Queries;
using Entradas.Aplicacion.Validadores;

namespace Entradas.Pruebas.Aplicacion.Validadores;

/// <summary>
/// Pruebas comprehensivas para ObtenerEntradaQueryValidator
/// Cubre todas las validaciones de negocio para queries de consulta
/// </summary>
public class ObtenerEntradaQueryValidatorTests
{
    private readonly ObtenerEntradaQueryValidator _validator;

    public ObtenerEntradaQueryValidatorTests()
    {
        _validator = new ObtenerEntradaQueryValidator();
    }

    [Fact]
    public void Validate_ConEntradaIdValido_DebeSerValido()
    {
        // Arrange
        var query = new ObtenerEntradaQuery(Guid.NewGuid());

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ConEntradaIdVacio_DebeSerInvalido()
    {
        // Arrange
        var query = new ObtenerEntradaQuery(Guid.Empty);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.EntradaId)
            .WithErrorMessage("El ID de la entrada debe ser un GUID válido");
    }

    [Fact]
    public void Validate_ConEntradaIdPorDefecto_DebeSerInvalido()
    {
        // Arrange
        var query = new ObtenerEntradaQuery(default(Guid));

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.EntradaId);
    }

    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("6ba7b810-9dad-11d1-80b4-00c04fd430c8")]
    [InlineData("6ba7b811-9dad-11d1-80b4-00c04fd430c8")]
    public void Validate_ConDiferentesGuidsValidos_DebeSerValido(string guidString)
    {
        // Arrange
        var entradaId = Guid.Parse(guidString);
        var query = new ObtenerEntradaQuery(entradaId);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldNotHaveValidationErrorFor(x => x.EntradaId);
    }

    [Fact]
    public void Validate_ConGuidGeneradoAleatoriamente_DebeSerValido()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            var query = new ObtenerEntradaQuery(Guid.NewGuid());

            // Act
            var resultado = _validator.TestValidate(query);

            // Assert
            resultado.ShouldNotHaveAnyValidationErrors();
        }
    }

    [Fact]
    public void Validate_ConMismoGuidMultiplesVeces_DebeSerValido()
    {
        // Arrange
        var entradaId = Guid.NewGuid();
        var query1 = new ObtenerEntradaQuery(entradaId);
        var query2 = new ObtenerEntradaQuery(entradaId);

        // Act
        var resultado1 = _validator.TestValidate(query1);
        var resultado2 = _validator.TestValidate(query2);

        // Assert
        resultado1.ShouldNotHaveAnyValidationErrors();
        resultado2.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Constructor_DebeCrearInstanciaValida()
    {
        // Arrange & Act
        var validator = new ObtenerEntradaQueryValidator();

        // Assert
        validator.Should().NotBeNull();
    }

    [Fact]
    public void Validate_ConQueryNula_DebeSerInvalido()
    {
        // Arrange
        ObtenerEntradaQuery query = null!;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _validator.TestValidate(query));
    }

    [Fact]
    public void Validate_ConEntradaIdVacio_DebeRetornarMensajeEspecifico()
    {
        // Arrange
        var query = new ObtenerEntradaQuery(Guid.Empty);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        var error = resultado.Errors.FirstOrDefault(e => e.PropertyName == nameof(query.EntradaId));
        error.Should().NotBeNull();
        error!.ErrorMessage.Should().BeOneOf(
            "El ID de la entrada es requerido", 
            "El ID de la entrada debe ser un GUID válido");
    }

    [Fact]
    public void Validate_ConMultiplesValidaciones_DebeValidarTodasLasReglas()
    {
        // Arrange
        var query = new ObtenerEntradaQuery(Guid.Empty);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.EntradaId);
        resultado.Errors.Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Validate_ConMultiplesQueriesValidas_DebeValidarTodas(int cantidad)
    {
        // Arrange & Act & Assert
        for (int i = 0; i < cantidad; i++)
        {
            var query = new ObtenerEntradaQuery(Guid.NewGuid());
            var resultado = _validator.TestValidate(query);
            resultado.ShouldNotHaveAnyValidationErrors();
        }
    }

    [Fact]
    public void Validate_ConGuidEspecifico_DebePreservarValor()
    {
        // Arrange
        var entradaIdEspecifico = new Guid("12345678-1234-5678-9012-123456789012");
        var query = new ObtenerEntradaQuery(entradaIdEspecifico);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldNotHaveAnyValidationErrors();
        query.EntradaId.Should().Be(entradaIdEspecifico);
    }

    [Fact]
    public void Validate_ConGuidMaxValue_DebeSerValido()
    {
        // Arrange
        var maxGuid = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var query = new ObtenerEntradaQuery(maxGuid);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ConGuidMinValue_DebeSerInvalido()
    {
        // Arrange
        var minGuid = new Guid("00000000-0000-0000-0000-000000000000");
        var query = new ObtenerEntradaQuery(minGuid);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.EntradaId);
    }

    [Fact]
    public void Validate_ConDiferentesFormatos_DebeValidarCorrectamente()
    {
        // Arrange
        var guids = new[]
        {
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Guid.Parse("{550e8400-e29b-41d4-a716-446655440001}"),
            new Guid("550e8400-e29b-41d4-a716-446655440002")
        };

        foreach (var guid in guids)
        {
            var query = new ObtenerEntradaQuery(guid);

            // Act
            var resultado = _validator.TestValidate(query);

            // Assert
            resultado.ShouldNotHaveAnyValidationErrors();
        }
    }
}