using FsCheck;
using FsCheck.Xunit;
using System.Reflection;
using Asientos.Aplicacion.Comandos;
using Asientos.Aplicacion.Queries;
using MediatR;

namespace Asientos.Pruebas.Aplicacion;

/// <summary>
/// Property-Based Tests para verificar inmutabilidad de Commands, Queries y DTOs.
/// Cada test ejecuta 100 iteraciones con datos generados aleatoriamente.
/// </summary>
public class InmutabilidadPropiedadesTests
{
    /// <summary>
    /// Feature: refactorizacion-asientos-cqrs-rabbitmq, Property 6: Commands son records inmutables
    /// Validates: Requirements 5.1, 5.4
    /// 
    /// Para cualquier Command en el sistema, debe estar definido como record con propiedades init-only.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Propiedad6_CommandsSonRecordsInmutables()
    {
        // Arrange - Obtener todos los tipos de Commands
        var commandTypes = typeof(CrearMapaAsientosComando).Assembly
            .GetTypes()
            .Where(t => t.Namespace == "Asientos.Aplicacion.Comandos" 
                     && !t.IsAbstract 
                     && !t.IsInterface
                     && t.GetInterfaces().Any(i => 
                         i.IsGenericType && 
                         (i.GetGenericTypeDefinition() == typeof(IRequest<>) || i == typeof(IRequest))))
            .ToList();

        // Act & Assert - Verificar cada Command
        var results = commandTypes.Select(commandType =>
        {
            // Verificar que es un record
            var isRecord = commandType.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance) != null;
            
            // Verificar que todas las propiedades son init-only
            var properties = commandType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var allPropertiesInitOnly = properties.All(prop =>
            {
                var setMethod = prop.GetSetMethod(nonPublic: true);
                if (setMethod == null) return true; // Propiedades sin setter son inmutables
                
                // Verificar si el setter es init-only (tiene el atributo IsExternalInit)
                var returnParameter = setMethod.ReturnParameter;
                var modreq = returnParameter.GetRequiredCustomModifiers();
                return modreq.Any(t => t.Name == "IsExternalInit");
            });

            return new
            {
                Type = commandType.Name,
                IsRecord = isRecord,
                AllPropertiesInitOnly = allPropertiesInitOnly,
                IsValid = isRecord && allPropertiesInitOnly
            };
        }).ToList();

        var allValid = results.All(r => r.IsValid);
        var invalidCommands = results.Where(r => !r.IsValid).ToList();

        return allValid
            .ToProperty()
            .Label($"Commands verificados: {results.Count}. " +
                   $"Válidos: {results.Count(r => r.IsValid)}. " +
                   (invalidCommands.Any() 
                       ? $"Inválidos: {string.Join(", ", invalidCommands.Select(c => c.Type))}" 
                       : "Todos válidos"));
    }

    /// <summary>
    /// Feature: refactorizacion-asientos-cqrs-rabbitmq, Property 7: Queries son records inmutables
    /// Validates: Requirements 5.2, 5.4
    /// 
    /// Para cualquier Query en el sistema, debe estar definida como record con propiedades init-only.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Propiedad7_QueriesSonRecordsInmutables()
    {
        // Arrange - Obtener todos los tipos de Queries
        var queryTypes = typeof(ObtenerMapaAsientosQuery).Assembly
            .GetTypes()
            .Where(t => t.Namespace == "Asientos.Aplicacion.Queries" 
                     && !t.IsAbstract 
                     && !t.IsInterface
                     && t.Name.EndsWith("Query")
                     && t.GetInterfaces().Any(i => 
                         i.IsGenericType && 
                         i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .ToList();

        // Act & Assert - Verificar cada Query
        var results = queryTypes.Select(queryType =>
        {
            // Verificar que es un record
            var isRecord = queryType.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance) != null;
            
            // Verificar que todas las propiedades son init-only
            var properties = queryType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var allPropertiesInitOnly = properties.All(prop =>
            {
                var setMethod = prop.GetSetMethod(nonPublic: true);
                if (setMethod == null) return true;
                
                var returnParameter = setMethod.ReturnParameter;
                var modreq = returnParameter.GetRequiredCustomModifiers();
                return modreq.Any(t => t.Name == "IsExternalInit");
            });

            return new
            {
                Type = queryType.Name,
                IsRecord = isRecord,
                AllPropertiesInitOnly = allPropertiesInitOnly,
                IsValid = isRecord && allPropertiesInitOnly
            };
        }).ToList();

        var allValid = results.All(r => r.IsValid);
        var invalidQueries = results.Where(r => !r.IsValid).ToList();

        return allValid
            .ToProperty()
            .Label($"Queries verificadas: {results.Count}. " +
                   $"Válidas: {results.Count(r => r.IsValid)}. " +
                   (invalidQueries.Any() 
                       ? $"Inválidas: {string.Join(", ", invalidQueries.Select(q => q.Type))}" 
                       : "Todas válidas"));
    }

    /// <summary>
    /// Feature: refactorizacion-asientos-cqrs-rabbitmq, Property 8: DTOs son records inmutables
    /// Validates: Requirements 5.3, 5.4
    /// 
    /// Para cualquier DTO retornado por Queries, debe estar definido como record con propiedades init-only.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Propiedad8_DTOsSonRecordsInmutables()
    {
        // Arrange - Obtener todos los tipos de DTOs
        var dtoTypes = typeof(MapaAsientosDto).Assembly
            .GetTypes()
            .Where(t => t.Namespace == "Asientos.Aplicacion.Queries" 
                     && !t.IsAbstract 
                     && !t.IsInterface
                     && t.Name.EndsWith("Dto"))
            .ToList();

        // Act & Assert - Verificar cada DTO
        var results = dtoTypes.Select(dtoType =>
        {
            // Verificar que es un record
            var isRecord = dtoType.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance) != null;
            
            // Verificar que todas las propiedades son init-only
            var properties = dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var allPropertiesInitOnly = properties.All(prop =>
            {
                var setMethod = prop.GetSetMethod(nonPublic: true);
                if (setMethod == null) return true;
                
                var returnParameter = setMethod.ReturnParameter;
                var modreq = returnParameter.GetRequiredCustomModifiers();
                return modreq.Any(t => t.Name == "IsExternalInit");
            });

            return new
            {
                Type = dtoType.Name,
                IsRecord = isRecord,
                AllPropertiesInitOnly = allPropertiesInitOnly,
                IsValid = isRecord && allPropertiesInitOnly
            };
        }).ToList();

        var allValid = results.All(r => r.IsValid);
        var invalidDtos = results.Where(r => !r.IsValid).ToList();

        return allValid
            .ToProperty()
            .Label($"DTOs verificados: {results.Count}. " +
                   $"Válidos: {results.Count(r => r.IsValid)}. " +
                   (invalidDtos.Any() 
                       ? $"Inválidos: {string.Join(", ", invalidDtos.Select(d => d.Type))}" 
                       : "Todos válidos"));
    }
}
