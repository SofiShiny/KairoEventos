using FluentAssertions;
using FluentValidation.TestHelper;
using Entradas.Aplicacion.Queries;
using Entradas.Aplicacion.Validadores;

namespace Entradas.Pruebas.Aplicacion.Validadores;

/// <summary>
/// Pruebas comprehensivas para ObtenerEntradasPorUsuarioQueryValidator
/// Cubre todas las validaciones de negocio para queries de consulta por usuario
/// </summary>
public class ObtenerEntradasPorUsuarioQueryValidatorTests
{
    private readonly ObtenerEntradasPorUsuarioQueryValidator _validator;

    public ObtenerEntradasPorUsuarioQueryValidatorTests()
    {
        _validator = new ObtenerEntradasPorUsuarioQueryValidator();
    }

    [Fact]
    public void Validate_ConUsuarioIdValido_DebeSerValido()
    {
        // Arrange
        var query = new ObtenerEntradasPorUsuarioQuery(Guid.NewGuid());

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ConUsuarioIdVacio_DebeSerInvalido()
    {
        // Arrange
        var query = new ObtenerEntradasPorUsuarioQuery(Guid.Empty);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.UsuarioId)
            .WithErrorMessage("El ID del usuario debe ser un GUID válido");
    }

    [Fact]
    public void Validate_ConUsuarioIdPorDefecto_DebeSerInvalido()
    {
        // Arrange
        var query = new ObtenerEntradasPorUsuarioQuery(default(Guid));

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.UsuarioId);
    }

    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("6ba7b810-9dad-11d1-80b4-00c04fd430c8")]
    [InlineData("6ba7b811-9dad-11d1-80b4-00c04fd430c8")]
    public void Validate_ConDiferentesGuidsValidos_DebeSerValido(string guidString)
    {
        // Arrange
        var usuarioId = Guid.Parse(guidString);
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioId);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldNotHaveValidationErrorFor(x => x.UsuarioId);
    }

    [Fact]
    public void Validate_ConGuidGeneradoAleatoriamente_DebeSerValido()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            var query = new ObtenerEntradasPorUsuarioQuery(Guid.NewGuid());

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
        var usuarioId = Guid.NewGuid();
        var query1 = new ObtenerEntradasPorUsuarioQuery(usuarioId);
        var query2 = new ObtenerEntradasPorUsuarioQuery(usuarioId);

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
        var validator = new ObtenerEntradasPorUsuarioQueryValidator();

        // Assert
        validator.Should().NotBeNull();
    }

    [Fact]
    public void Validate_ConQueryNula_DebeSerInvalido()
    {
        // Arrange
        ObtenerEntradasPorUsuarioQuery query = null!;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _validator.TestValidate(query));
    }

    [Fact]
    public void Validate_ConUsuarioIdVacio_DebeRetornarMensajeEspecifico()
    {
        // Arrange
        var query = new ObtenerEntradasPorUsuarioQuery(Guid.Empty);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        var error = resultado.Errors.FirstOrDefault(e => e.PropertyName == nameof(query.UsuarioId));
        error.Should().NotBeNull();
        error!.ErrorMessage.Should().BeOneOf(
            "El ID del usuario es requerido", 
            "El ID del usuario debe ser un GUID válido");
    }

    [Fact]
    public void Validate_ConMultiplesValidaciones_DebeValidarTodasLasReglas()
    {
        // Arrange
        var query = new ObtenerEntradasPorUsuarioQuery(Guid.Empty);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.UsuarioId);
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
            var query = new ObtenerEntradasPorUsuarioQuery(Guid.NewGuid());
            var resultado = _validator.TestValidate(query);
            resultado.ShouldNotHaveAnyValidationErrors();
        }
    }

    [Fact]
    public void Validate_ConGuidEspecifico_DebePreservarValor()
    {
        // Arrange
        var usuarioIdEspecifico = new Guid("12345678-1234-5678-9012-123456789012");
        var query = new ObtenerEntradasPorUsuarioQuery(usuarioIdEspecifico);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldNotHaveAnyValidationErrors();
        query.UsuarioId.Should().Be(usuarioIdEspecifico);
    }

    [Fact]
    public void Validate_ConGuidMaxValue_DebeSerValido()
    {
        // Arrange
        var maxGuid = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var query = new ObtenerEntradasPorUsuarioQuery(maxGuid);

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
        var query = new ObtenerEntradasPorUsuarioQuery(minGuid);

        // Act
        var resultado = _validator.TestValidate(query);

        // Assert
        resultado.ShouldHaveValidationErrorFor(x => x.UsuarioId);
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
            var query = new ObtenerEntradasPorUsuarioQuery(guid);

            // Act
            var resultado = _validator.TestValidate(query);

            // Assert
            resultado.ShouldNotHaveAnyValidationErrors();
        }
    }

    [Fact]
    public void Validate_ConUsuariosDistintos_DebeValidarIndependientemente()
    {
        // Arrange
        var usuario1 = Guid.NewGuid();
        var usuario2 = Guid.NewGuid();
        var usuario3 = Guid.NewGuid();

        var queries = new[]
        {
            new ObtenerEntradasPorUsuarioQuery(usuario1),
            new ObtenerEntradasPorUsuarioQuery(usuario2),
            new ObtenerEntradasPorUsuarioQuery(usuario3)
        };

        // Act & Assert
        foreach (var query in queries)
        {
            var resultado = _validator.TestValidate(query);
            resultado.ShouldNotHaveAnyValidationErrors();
        }
    }

    [Fact]
    public void Validate_ConValidacionConcurrente_DebeSerThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();
        var resultados = new List<bool>();
        var lockObject = new object();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var query = new ObtenerEntradasPorUsuarioQuery(Guid.NewGuid());
                var resultado = _validator.TestValidate(query);
                
                lock (lockObject)
                {
                    resultados.Add(!resultado.Errors.Any());
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        resultados.Should().HaveCount(10);
        resultados.Should().AllBeEquivalentTo(true);
    }

    [Fact]
    public void Validate_ConPerformanceTest_DebeSerRapido()
    {
        // Arrange
        var queries = new List<ObtenerEntradasPorUsuarioQuery>();
        for (int i = 0; i < 1000; i++)
        {
            queries.Add(new ObtenerEntradasPorUsuarioQuery(Guid.NewGuid()));
        }

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        foreach (var query in queries)
        {
            _validator.TestValidate(query);
        }
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Debe validar 1000 queries en menos de 1 segundo
    }
}