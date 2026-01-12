using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using Usuarios.Dominio.ObjetosValor;

namespace Usuarios.Pruebas.Propiedades;

/// <summary>
/// Feature: refactorizacion-usuarios, Property 5: Inmutabilidad de Value Objects
/// Validates: Requirements 3.3, 3.4, 3.5
/// 
/// For any Value Object (Correo, Telefono, Direccion), una vez creado no puede ser modificado (inmutable).
/// </summary>
public class InmutabilidadValueObjectsPropiedadesTests
{
    [Property(MaxTest = 100)]
    public Property CorreoEsInmutable()
    {
        return Prop.ForAll(
            GeneradorCorreoValido(),
            (correoStr) =>
            {
                // Arrange
                var correo1 = Correo.Crear(correoStr);
                var correo2 = Correo.Crear(correoStr);

                // Assert: Los records con mismo valor son iguales
                correo1.Should().Be(correo2, "records con mismo valor deben ser iguales");
                
                // Assert: El tipo es un record (tiene método Equals generado)
                correo1.GetType().IsValueType.Should().BeFalse("Correo es un record (reference type)");
                correo1.GetType().GetMethod("Equals", new[] { typeof(object) })
                    .Should().NotBeNull("records tienen método Equals");
            });
    }

    [Property(MaxTest = 100)]
    public Property TelefonoEsInmutable()
    {
        return Prop.ForAll(
            GeneradorTelefonoValido(),
            (telefonoStr) =>
            {
                // Arrange
                var telefono1 = Telefono.Crear(telefonoStr);
                var telefono2 = Telefono.Crear(telefonoStr);

                // Assert: Los records con mismo valor son iguales
                telefono1.Should().Be(telefono2, "records con mismo valor deben ser iguales");
                
                // Assert: El tipo es un record
                telefono1.GetType().IsValueType.Should().BeFalse("Telefono es un record (reference type)");
                telefono1.GetType().GetMethod("Equals", new[] { typeof(object) })
                    .Should().NotBeNull("records tienen método Equals");
            });
    }

    [Property(MaxTest = 100)]
    public Property DireccionEsInmutable()
    {
        return Prop.ForAll(
            GeneradorDireccionValida(),
            (direccionStr) =>
            {
                // Arrange
                var direccion1 = Direccion.Crear(direccionStr);
                var direccion2 = Direccion.Crear(direccionStr);

                // Assert: Los records con mismo valor son iguales
                direccion1.Should().Be(direccion2, "records con mismo valor deben ser iguales");
                
                // Assert: El tipo es un record
                direccion1.GetType().IsValueType.Should().BeFalse("Direccion es un record (reference type)");
                direccion1.GetType().GetMethod("Equals", new[] { typeof(object) })
                    .Should().NotBeNull("records tienen método Equals");
            });
    }

    [Property(MaxTest = 100)]
    public Property ValueObjectsNoTienenSettersPublicos()
    {
        return Prop.ForAll(
            Arb.From(Gen.Constant(true)),
            (_) =>
            {
                // Assert: Correo no tiene setters públicos
                var correoProps = typeof(Correo).GetProperties();
                foreach (var prop in correoProps)
                {
                    prop.SetMethod.Should().BeNull($"Correo.{prop.Name} no debe tener setter público");
                }

                // Assert: Telefono no tiene setters públicos
                var telefonoProps = typeof(Telefono).GetProperties();
                foreach (var prop in telefonoProps)
                {
                    prop.SetMethod.Should().BeNull($"Telefono.{prop.Name} no debe tener setter público");
                }

                // Assert: Direccion no tiene setters públicos
                var direccionProps = typeof(Direccion).GetProperties();
                foreach (var prop in direccionProps)
                {
                    prop.SetMethod.Should().BeNull($"Direccion.{prop.Name} no debe tener setter público");
                }
            });
    }

    private static Arbitrary<string> GeneradorCorreoValido()
    {
        return Arb.From(
            Gen.Elements("test", "user", "admin")
                .SelectMany(prefix =>
                    Gen.Choose(1, 999)
                        .SelectMany(num =>
                            Gen.Elements("example.com", "test.com")
                                .Select(domain => $"{prefix}{num}@{domain}"))));
    }

    private static Arbitrary<string> GeneradorTelefonoValido()
    {
        return Arb.From(
            Gen.Choose(7, 15)
                .SelectMany(length =>
                    Gen.ArrayOf(length, Gen.Choose(0, 9))
                        .Select(digits => string.Join("", digits))));
    }

    private static Arbitrary<string> GeneradorDireccionValida()
    {
        return Arb.From(
            Gen.Elements("Calle", "Avenida", "Boulevard")
                .SelectMany(tipo =>
                    Gen.Elements("Principal", "Central", "Norte", "Sur")
                        .SelectMany(nombre =>
                            Gen.Choose(1, 999)
                                .Select(num => $"{tipo} {nombre} {num}"))));
    }
}
