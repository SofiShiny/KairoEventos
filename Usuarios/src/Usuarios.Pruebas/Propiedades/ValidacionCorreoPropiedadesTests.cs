using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using Usuarios.Dominio.ObjetosValor;

namespace Usuarios.Pruebas.Propiedades;

/// <summary>
/// Feature: refactorizacion-usuarios, Property 3: Validación de Correo
/// Validates: Requirements 3.3, 7.2
/// 
/// For any correo almacenado en el sistema, debe tener un formato válido de email (contiene @ y dominio válido).
/// </summary>
public class ValidacionCorreoPropiedadesTests
{
    [Property(MaxTest = 100)]
    public Property CorreosValidosSonAceptados()
    {
        return Prop.ForAll(
            GeneradorCorreoValido(),
            (correoStr) =>
            {
                // Act
                var correo = Correo.Crear(correoStr);

                // Assert
                correo.Should().NotBeNull("correos válidos deben ser aceptados");
                correo.Valor.Should().Contain("@", "un correo válido debe contener @");
                correo.Valor.Should().Contain(".", "un correo válido debe contener un dominio");
            });
    }

    [Property(MaxTest = 100)]
    public Property CorreosInvalidosSonRechazados()
    {
        return Prop.ForAll(
            GeneradorCorreoInvalido(),
            (correoInvalido) =>
            {
                // Act & Assert
                Action act = () => Correo.Crear(correoInvalido);
                act.Should().Throw<ArgumentException>("correos inválidos deben ser rechazados");
            });
    }

    [Property(MaxTest = 100)]
    public Property CorreosSeNormalizanAMinusculas()
    {
        return Prop.ForAll(
            GeneradorCorreoValido(),
            (correoStr) =>
            {
                // Arrange: Crear correo con mayúsculas
                var correoMixto = correoStr.ToUpperInvariant();

                // Act
                var correo = Correo.Crear(correoMixto);

                // Assert
                correo.Valor.Should().Be(correoStr.ToLowerInvariant(),
                    "los correos deben normalizarse a minúsculas");
            });
    }

    private static Arbitrary<string> GeneradorCorreoValido()
    {
        return Arb.From(
            Gen.Elements("test", "user", "admin", "demo", "john", "jane", "alice", "bob")
                .SelectMany(prefix =>
                    Gen.Choose(1, 999)
                        .SelectMany(num =>
                            Gen.Elements("example.com", "test.com", "mail.com", "domain.org")
                                .Select(domain => $"{prefix}{num}@{domain}"))));
    }

    private static Arbitrary<string> GeneradorCorreoInvalido()
    {
        return Arb.From(
            Gen.OneOf(
                // Sin @
                Gen.Elements("test", "user123", "admin.com").Select(s => s),
                // Sin dominio
                Gen.Elements("test@", "user@", "admin@").Select(s => s),
                // Sin parte local
                Gen.Elements("@example.com", "@test.com").Select(s => s),
                // Vacío
                Gen.Constant(""),
                // Solo espacios
                Gen.Constant("   "),
                // Múltiples @
                Gen.Elements("test@@example.com", "user@test@com").Select(s => s)
            ));
    }
}
