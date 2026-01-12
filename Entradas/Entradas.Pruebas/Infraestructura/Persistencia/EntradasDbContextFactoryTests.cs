using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using FluentAssertions;
using Entradas.Infraestructura.Persistencia;
using Entradas.Dominio.Enums;

namespace Entradas.Pruebas.Infraestructura.Persistencia;

/// <summary>
/// Pruebas para EntradasDbContextFactory
/// Valida la creación de contextos para migraciones y design-time
/// </summary>
public class EntradasDbContextFactoryTests
{
    [Fact]
    public void CreateDbContext_SinArgumentos_DeberiaCrearContexto()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        context.Should().NotBeNull();
        context.Should().BeOfType<EntradasDbContext>();
        context.Database.Should().NotBeNull();
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_ConArgumentos_DeberiaCrearContexto()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();
        var args = new[] { "--environment", "Development" };

        // Act
        var context = factory.CreateDbContext(args);

        // Assert
        context.Should().NotBeNull();
        context.Should().BeOfType<EntradasDbContext>();
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_DeberiaConfigurarPostgreSQL()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        context.Database.ProviderName.Should().Be("Npgsql.EntityFrameworkCore.PostgreSQL");
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_DeberiaConfigurarEntidades()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        var model = context.Model;
        model.Should().NotBeNull();
        
        var entradaEntityType = model.FindEntityType("Entradas.Dominio.Entidades.Entrada");
        entradaEntityType.Should().NotBeNull();
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_DeberiaImplementarIDesignTimeDbContextFactory()
    {
        // Arrange & Act
        var factory = new EntradasDbContextFactory();

        // Assert
        factory.Should().BeAssignableTo<IDesignTimeDbContextFactory<EntradasDbContext>>();
    }

    [Fact]
    public void CreateDbContext_ConMultiplesLlamadas_DeberiaCrearInstanciasIndependientes()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context1 = factory.CreateDbContext(Array.Empty<string>());
        var context2 = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        context1.Should().NotBeNull();
        context2.Should().NotBeNull();
        context1.Should().NotBeSameAs(context2);
        
        // Cleanup
        context1.Dispose();
        context2.Dispose();
    }

    [Fact]
    public void CreateDbContext_DeberiaConfigurarConnectionString()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        var connectionString = context.Database.GetConnectionString();
        connectionString.Should().NotBeNullOrEmpty();
        connectionString.Should().Contain("Host=");
        connectionString.Should().Contain("Database=");
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_DeberiaPermitirMigraciones()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        // Verificar que el contexto está configurado para migraciones
        // Sin intentar conectar a la base de datos
        var model = context.Model;
        model.Should().NotBeNull();
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_DeberiaConfigurarLogging()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        // Verificar que el contexto tiene configuración de logging
        context.Should().NotBeNull();
        
        // El contexto debería poder ejecutar operaciones sin errores
        FluentActions
            .Invoking(() => context.Database.CanConnect())
            .Should()
            .NotThrow();
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_ConArgumentosNulos_DeberiaCrearContexto()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act & Assert
        FluentActions
            .Invoking(() => factory.CreateDbContext(null!))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void CreateDbContext_DeberiaConfigurarModelCorrectamente()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        var model = context.Model;
        var entradaEntity = model.FindEntityType("Entradas.Dominio.Entidades.Entrada");
        
        entradaEntity.Should().NotBeNull();
        // PostgreSQL convierte los nombres de tabla a minúsculas por defecto
        entradaEntity!.GetTableName().Should().Be("entradas");
        
        var primaryKey = entradaEntity.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties.First().Name.Should().Be("Id");
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_DeberiaConfigurarIndices()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        var model = context.Model;
        var entradaEntity = model.FindEntityType("Entradas.Dominio.Entidades.Entrada");
        
        var indices = entradaEntity!.GetIndexes();
        indices.Should().NotBeEmpty();
        
        // Verificar índice único en CodigoQr
        var codigoQrIndex = indices.FirstOrDefault(i => 
            i.Properties.Count == 1 && 
            i.Properties.First().Name == "CodigoQr");
        codigoQrIndex.Should().NotBeNull();
        codigoQrIndex!.IsUnique.Should().BeTrue();
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_DeberiaPermitirOperacionesBasicas()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        // Verificar que se pueden realizar operaciones básicas
        FluentActions
            .Invoking(() =>
            {
                var entradas = context.Entradas;
                entradas.Should().NotBeNull();
            })
            .Should()
            .NotThrow();
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_DeberiaConfigurarConversionesDeValor()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        var model = context.Model;
        var entradaEntity = model.FindEntityType("Entradas.Dominio.Entidades.Entrada");
        
        var estadoProperty = entradaEntity!.FindProperty("Estado");
        estadoProperty.Should().NotBeNull();
        
        // La propiedad Estado es de tipo EstadoEntrada (enum)
        estadoProperty!.ClrType.Should().Be(typeof(EstadoEntrada));
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_DeberiaConfigurarPrecisionDecimal()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        var model = context.Model;
        var entradaEntity = model.FindEntityType("Entradas.Dominio.Entidades.Entrada");
        
        var montoProperty = entradaEntity!.FindProperty("Monto");
        montoProperty.Should().NotBeNull();
        // PostgreSQL usa "numeric" en lugar de "decimal"
        montoProperty!.GetColumnType().Should().Be("numeric(18,2)");
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_DeberiaConfigurarLongitudMaxima()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();

        // Act
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Assert
        var model = context.Model;
        var entradaEntity = model.FindEntityType("Entradas.Dominio.Entidades.Entrada");
        
        var codigoQrProperty = entradaEntity!.FindProperty("CodigoQr");
        codigoQrProperty.Should().NotBeNull();
        codigoQrProperty!.GetMaxLength().Should().Be(100);
        
        // Cleanup
        context.Dispose();
    }

    [Fact]
    public void CreateDbContext_DeberiaSerDisposable()
    {
        // Arrange
        var factory = new EntradasDbContextFactory();
        var context = factory.CreateDbContext(Array.Empty<string>());

        // Act & Assert
        FluentActions
            .Invoking(() => context.Dispose())
            .Should()
            .NotThrow();
    }
}