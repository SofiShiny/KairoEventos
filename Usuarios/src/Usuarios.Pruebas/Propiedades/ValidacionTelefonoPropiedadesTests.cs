using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using Usuarios.Dominio.ObjetosValor;

namespace Usuarios.Pruebas.Propiedades;

/// <summary>
/// Feature: refactorizacion-usuarios, Property 4: Validación de Teléfono
/// Validates: Requirements 3.4, 7.3
/// 
/// For any teléfono almacenado en el sistema, debe contener solo dígitos y tener entre 7 y 15 caracteres.
/// </summary>
public class ValidacionTelefonoPropiedadesTests
{
    [Property(MaxTest = 100)]
    public Property TelefonosValidosSonAceptados()
    {
        return Prop.ForAll(
            GeneradorTelefonoValido(),
            (telefonoStr) =>
            {
                // Act
                var telefono = Telefono.Crear(telefonoStr);

                // Assert
                telefono.Should().NotBeNull("teléfonos válidos deben ser aceptados");
                telefono.Valor.Should().MatchRegex("^[0-9]+$", "el teléfono debe contener solo dígitos");
                telefono.Valor.Length.Should().BeInRange(7, 15, "el teléfono debe tener entre 7 y 15 dígitos");
            });
    }

    [Property(MaxTest = 100)]
    public Property TelefonosMuyCortosSonRechazados()
    {
        return Prop.ForAll(
            GeneradorTelefonoMuyCorto(),
            (telefonoCorto) =>
            {
                // Act & Assert
                Action act = () => Telefono.Crear(telefonoCorto);
                act.Should().Throw<ArgumentException>("teléfonos con menos de 7 dígitos deben ser rechazados");
            });
    }

    [Property(MaxTest = 100)]
    public Property TelefonosMuyLargosSonRechazados()
    {
        return Prop.ForAll(
            GeneradorTelefonoMuyLargo(),
            (telefonoLargo) =>
            {
                // Act & Assert
                Action act = () => Telefono.Crear(telefonoLargo);
                act.Should().Throw<ArgumentException>("teléfonos con más de 15 dígitos deben ser rechazados");
            });
    }

    [Property(MaxTest = 100)]
    public Property TelefonosConCaracteresEspecialesSonLimpiados()
    {
        return Prop.ForAll(
            GeneradorTelefonoConCaracteresEspeciales(),
            (telefonoConEspeciales) =>
            {
                // Act
                var telefono = Telefono.Crear(telefonoConEspeciales);

                // Assert
                telefono.Valor.Should().MatchRegex("^[0-9]+$", 
                    "los caracteres especiales deben ser removidos, dejando solo dígitos");
            });
    }

    private static Arbitrary<string> GeneradorTelefonoValido()
    {
        return Arb.From(
            Gen.Choose(7, 15)
                .SelectMany(length =>
                    Gen.ArrayOf(length, Gen.Choose(0, 9))
                        .Select(digits => string.Join("", digits))));
    }

    private static Arbitrary<string> GeneradorTelefonoMuyCorto()
    {
        return Arb.From(
            Gen.Choose(1, 6)
                .SelectMany(length =>
                    Gen.ArrayOf(length, Gen.Choose(0, 9))
                        .Select(digits => string.Join("", digits))));
    }

    private static Arbitrary<string> GeneradorTelefonoMuyLargo()
    {
        return Arb.From(
            Gen.Choose(16, 25)
                .SelectMany(length =>
                    Gen.ArrayOf(length, Gen.Choose(0, 9))
                        .Select(digits => string.Join("", digits))));
    }

    private static Arbitrary<string> GeneradorTelefonoConCaracteresEspeciales()
    {
        return Arb.From(
            Gen.Choose(7, 15)
                .SelectMany(length =>
                    Gen.ArrayOf(length, Gen.Choose(0, 9))
                        .Select(digits =>
                        {
                            var numero = string.Join("", digits);
                            // Agregar caracteres especiales comunes en teléfonos
                            var formatos = new[]
                            {
                                $"+{numero}",
                                $"({numero.Substring(0, 3)}) {numero.Substring(3)}",
                                $"{numero.Substring(0, 3)}-{numero.Substring(3)}",
                                $"{numero.Substring(0, 3)} {numero.Substring(3)}"
                            };
                            return formatos[new System.Random().Next(formatos.Length)];
                        })));
    }
}
