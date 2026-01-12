using Xunit;
using FluentAssertions;
using MediatR;
using System.Reflection;
using Asientos.Aplicacion.Comandos;
using Asientos.Aplicacion.Queries;

namespace Asientos.Pruebas.Aplicacion;

/// <summary>
/// Tests para validar que el patrón CQRS está correctamente implementado
/// Feature: refactorizacion-asientos-cqrs-rabbitmq
/// </summary>
public class CQRSValidationTests
{
    private readonly Assembly _aplicacionAssembly;

    public CQRSValidationTests()
    {
        _aplicacionAssembly = typeof(CrearMapaAsientosComando).Assembly;
    }

    [Fact]
    public void Commands_Should_Return_Only_Guid_Or_Unit()
    {
        // Arrange
        var commandTypes = _aplicacionAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .Where(t => t.Name.EndsWith("Comando"))
            .ToList();

        // Act & Assert
        foreach (var commandType in commandTypes)
        {
            var requestInterface = commandType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
            
            var returnType = requestInterface.GetGenericArguments()[0];
            
            // Commands deben retornar solo Guid o Unit
            (returnType == typeof(Guid) || returnType == typeof(Unit))
                .Should().BeTrue(
                    $"Command {commandType.Name} debe retornar Guid o Unit, pero retorna {returnType.Name}");
        }

        // Verificar que encontramos commands
        commandTypes.Should().NotBeEmpty("Debe haber al menos un Command en el sistema");
    }

    [Fact]
    public void Queries_Should_Return_DTOs()
    {
        // Arrange
        var queryTypes = _aplicacionAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .Where(t => t.Name.EndsWith("Query"))
            .ToList();

        // Act & Assert
        foreach (var queryType in queryTypes)
        {
            var requestInterface = queryType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
            
            var returnType = requestInterface.GetGenericArguments()[0];
            
            // Queries deben retornar DTOs (records que terminan en Dto)
            // o tipos nullable de DTOs
            var actualType = Nullable.GetUnderlyingType(returnType) ?? returnType;
            
            actualType.Name.Should().EndWith("Dto",
                $"Query {queryType.Name} debe retornar un DTO, pero retorna {returnType.Name}");
        }

        // Verificar que encontramos queries
        queryTypes.Should().NotBeEmpty("Debe haber al menos una Query en el sistema");
    }

    [Fact]
    public void Commands_Should_Be_Records()
    {
        // Arrange
        var commandTypes = _aplicacionAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("Comando"))
            .ToList();

        // Act & Assert
        foreach (var commandType in commandTypes)
        {
            // Los records tienen un método especial <Clone>$
            var isRecord = commandType.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance) != null;
            
            isRecord.Should().BeTrue(
                $"Command {commandType.Name} debe ser un record para garantizar inmutabilidad");
        }

        commandTypes.Should().NotBeEmpty();
    }

    [Fact]
    public void Queries_Should_Be_Records()
    {
        // Arrange
        var queryTypes = _aplicacionAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("Query"))
            .ToList();

        // Act & Assert
        foreach (var queryType in queryTypes)
        {
            // Los records tienen un método especial <Clone>$
            var isRecord = queryType.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance) != null;
            
            isRecord.Should().BeTrue(
                $"Query {queryType.Name} debe ser un record para garantizar inmutabilidad");
        }

        queryTypes.Should().NotBeEmpty();
    }

    [Fact]
    public void DTOs_Should_Be_Records()
    {
        // Arrange
        var dtoTypes = _aplicacionAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("Dto"))
            .ToList();

        // Act & Assert
        foreach (var dtoType in dtoTypes)
        {
            // Los records tienen un método especial <Clone>$
            var isRecord = dtoType.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance) != null;
            
            isRecord.Should().BeTrue(
                $"DTO {dtoType.Name} debe ser un record para garantizar inmutabilidad");
        }

        dtoTypes.Should().NotBeEmpty();
    }

    [Fact]
    public void Commands_And_Queries_Should_Have_Init_Only_Properties()
    {
        // Arrange
        var types = _aplicacionAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("Comando") || t.Name.EndsWith("Query") || t.Name.EndsWith("Dto"))
            .ToList();

        // Act & Assert
        foreach (var type in types)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var property in properties)
            {
                var setMethod = property.GetSetMethod(nonPublic: true);
                
                if (setMethod != null)
                {
                    // Verificar que es init-only (los records tienen esto automáticamente)
                    var isInitOnly = setMethod.ReturnParameter
                        .GetRequiredCustomModifiers()
                        .Any(t => t.Name == "IsExternalInit");
                    
                    isInitOnly.Should().BeTrue(
                        $"Propiedad {property.Name} en {type.Name} debe ser init-only para garantizar inmutabilidad");
                }
            }
        }

        types.Should().NotBeEmpty();
    }

    [Fact]
    public void CrearMapaAsientosComando_Should_Return_Guid()
    {
        // Arrange
        var commandType = typeof(CrearMapaAsientosComando);

        // Act
        var requestInterface = commandType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
        
        var returnType = requestInterface.GetGenericArguments()[0];

        // Assert
        returnType.Should().Be(typeof(Guid), 
            "CrearMapaAsientosComando debe retornar Guid, no la entidad completa");
    }

    [Fact]
    public void ObtenerMapaAsientosQuery_Should_Return_DTO()
    {
        // Arrange
        var queryType = typeof(ObtenerMapaAsientosQuery);

        // Act
        var requestInterface = queryType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
        
        var returnType = requestInterface.GetGenericArguments()[0];
        var actualType = Nullable.GetUnderlyingType(returnType) ?? returnType;

        // Assert
        actualType.Name.Should().EndWith("Dto",
            "ObtenerMapaAsientosQuery debe retornar un DTO inmutable");
    }

    [Fact]
    public void All_Commands_Should_Be_In_Comandos_Namespace()
    {
        // Arrange
        var commandTypes = _aplicacionAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("Comando"))
            .ToList();

        // Act & Assert
        foreach (var commandType in commandTypes)
        {
            commandType.Namespace.Should().Be("Asientos.Aplicacion.Comandos",
                $"Command {commandType.Name} debe estar en el namespace Asientos.Aplicacion.Comandos");
        }

        commandTypes.Should().NotBeEmpty();
    }

    [Fact]
    public void All_Queries_Should_Be_In_Queries_Namespace()
    {
        // Arrange
        var queryTypes = _aplicacionAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("Query"))
            .ToList();

        // Act & Assert
        foreach (var queryType in queryTypes)
        {
            queryType.Namespace.Should().Be("Asientos.Aplicacion.Queries",
                $"Query {queryType.Name} debe estar en el namespace Asientos.Aplicacion.Queries");
        }

        queryTypes.Should().NotBeEmpty();
    }
}
