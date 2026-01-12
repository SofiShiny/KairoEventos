using FluentAssertions;
using Entradas.Infraestructura.Servicios;
using Entradas.Dominio.Interfaces;

namespace Entradas.Pruebas.Infraestructura;

/// <summary>
/// Tests unitarios para GeneradorCodigoQr
/// Valida la generación correcta de códigos QR únicos y la validación de formato
/// </summary>
public class GeneradorCodigoQrTests
{
    private readonly IGeneradorCodigoQr _generador;

    public GeneradorCodigoQrTests()
    {
        _generador = new GeneradorCodigoQr();
    }

    [Fact]
    public void GenerarCodigoUnico_DebeGenerarCodigoConFormatoCorrecto()
    {
        // Act
        var codigo = _generador.GenerarCodigoUnico();

        // Assert
        codigo.Should().NotBeNullOrEmpty();
        codigo.Should().StartWith("TICKET-");
        codigo.Should().MatchRegex(@"^TICKET-[A-F0-9]{8}-\d{4}$");
    }

    [Fact]
    public void GenerarCodigoUnico_DebeGenerarCodigosUnicos()
    {
        // Arrange
        var codigos = new HashSet<string>();
        const int cantidadCodigos = 1000;

        // Act
        for (int i = 0; i < cantidadCodigos; i++)
        {
            var codigo = _generador.GenerarCodigoUnico();
            codigos.Add(codigo);
        }

        // Assert
        codigos.Should().HaveCount(cantidadCodigos, "todos los códigos generados deben ser únicos");
    }

    [Theory]
    [InlineData("TICKET-12345678-1234", true)]
    [InlineData("TICKET-ABCDEF12-9999", true)]
    [InlineData("TICKET-00000000-0000", true)]
    [InlineData("ticket-12345678-1234", false)] // minúsculas
    [InlineData("TICKET-1234567-1234", false)]  // guid muy corto
    [InlineData("TICKET-123456789-1234", false)] // guid muy largo
    [InlineData("TICKET-12345678-123", false)]   // random muy corto
    [InlineData("TICKET-12345678-12345", false)] // random muy largo
    [InlineData("TICKET-12345678", false)]       // falta random
    [InlineData("12345678-1234", false)]         // falta prefijo
    [InlineData("", false)]                      // string vacío
    [InlineData("TICKET-GGGGGGGG-1234", false)] // caracteres inválidos en guid
    [InlineData("TICKET-12345678-ABCD", false)] // caracteres inválidos en random
    public void ValidarFormato_DebeValidarFormatoCorrectamente(string codigoQr, bool esperado)
    {
        // Act
        var resultado = _generador.ValidarFormato(codigoQr);

        // Assert
        resultado.Should().Be(esperado);
    }

    [Fact]
    public void ValidarFormato_ConCodigoNull_DebeRetornarFalse()
    {
        // Act
        var resultado = _generador.ValidarFormato(null!);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void ValidarFormato_ConCodigoGenerado_DebeRetornarTrue()
    {
        // Arrange
        var codigo = _generador.GenerarCodigoUnico();

        // Act
        var resultado = _generador.ValidarFormato(codigo);

        // Assert
        resultado.Should().BeTrue("el código generado debe tener formato válido");
    }

    [Fact]
    public void GenerarCodigoUnico_DebeUsarComponentesCriptograficamenteSegurosPorConsistencia()
    {
        // Arrange
        var codigos = new List<string>();
        const int cantidadPruebas = 100;

        // Act - Generar múltiples códigos para verificar distribución
        for (int i = 0; i < cantidadPruebas; i++)
        {
            codigos.Add(_generador.GenerarCodigoUnico());
        }

        // Assert - Verificar que hay variación en los códigos (no son secuenciales)
        var parteGuid = codigos.Select(c => c.Split('-')[1]).ToList();
        var parteRandom = codigos.Select(c => c.Split('-')[2]).ToList();

        parteGuid.Distinct().Should().HaveCountGreaterThan((int)(cantidadPruebas * 0.95), 
            "las partes GUID deben ser altamente únicas");
        parteRandom.Distinct().Should().HaveCountGreaterThan((int)(cantidadPruebas * 0.8), 
            "las partes random deben mostrar buena distribución");
    }
}