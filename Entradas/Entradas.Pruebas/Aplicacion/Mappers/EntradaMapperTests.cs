using FluentAssertions;
using Entradas.Aplicacion.DTOs;
using Entradas.Aplicacion.Mappers;
using Entradas.Dominio.Entidades;
using Entradas.Dominio.Enums;

namespace Entradas.Pruebas.Aplicacion.Mappers;

/// <summary>
/// Pruebas comprehensivas para EntradaMapper
/// Cubre mapeo bidireccional y casos edge
/// </summary>
public class EntradaMapperTests
{
    [Fact]
    public void ToDto_ConEntradaCompleta_DebeMappearCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var monto = 150.75m;
        var codigoQr = "TICKET-ABC123-4567";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, asientoId, codigoQr);

        // Act
        var dto = EntradaMapper.ToDto(entrada);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(entrada.Id);
        dto.EventoId.Should().Be(eventoId);
        dto.UsuarioId.Should().Be(usuarioId);
        dto.AsientoId.Should().Be(asientoId);
        dto.Monto.Should().Be(monto);
        dto.CodigoQr.Should().Be(codigoQr);
        dto.Estado.Should().Be(EstadoEntrada.PendientePago);
        dto.FechaCompra.Should().Be(entrada.FechaCompra);
    }

    [Fact]
    public void ToDto_ConEntradaGeneral_DebeMappearAsientoIdComoNull()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 100.00m;
        var codigoQr = "TICKET-DEF456-7890";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);

        // Act
        var dto = EntradaMapper.ToDto(entrada);

        // Assert
        dto.Should().NotBeNull();
        dto.AsientoId.Should().BeNull();
        dto.EventoId.Should().Be(eventoId);
        dto.UsuarioId.Should().Be(usuarioId);
        dto.Monto.Should().Be(monto);
        dto.CodigoQr.Should().Be(codigoQr);
    }

    [Fact]
    public void ToDto_ConEntradaPagada_DebeMappearEstadoCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 200.00m;
        var codigoQr = "TICKET-GHI789-0123";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
        entrada.ConfirmarPago(); // Cambiar estado

        // Act
        var dto = EntradaMapper.ToDto(entrada);

        // Assert
        dto.Estado.Should().Be(EstadoEntrada.Pagada);
    }

    [Fact]
    public void ToDto_ConEntradaCancelada_DebeMappearEstadoCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 75.50m;
        var codigoQr = "TICKET-JKL012-3456";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
        entrada.Cancelar(); // Cambiar estado

        // Act
        var dto = EntradaMapper.ToDto(entrada);

        // Assert
        dto.Estado.Should().Be(EstadoEntrada.Cancelada);
    }

    [Fact]
    public void ToDto_ConEntradaUsada_DebeMappearEstadoCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 300.00m;
        var codigoQr = "TICKET-MNO345-6789";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, null, codigoQr);
        entrada.ConfirmarPago(); // Primero pagar
        entrada.MarcarComoUsada(); // Luego usar

        // Act
        var dto = EntradaMapper.ToDto(entrada);

        // Assert
        dto.Estado.Should().Be(EstadoEntrada.Usada);
    }

    [Fact]
    public void ToDto_ConEntradaNula_DebeLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => EntradaMapper.ToDto((Entrada)null!));
    }

    [Fact]
    public void ToEntradaCreadaDto_ConEntradaCompleta_DebeMappearCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var monto = 250.25m;
        var codigoQr = "TICKET-PQR678-9012";

        var entrada = Entrada.Crear(eventoId, usuarioId, monto, asientoId, codigoQr);

        // Act
        var dto = EntradaMapper.ToEntradaCreadaDto(entrada);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(entrada.Id);
        dto.EventoId.Should().Be(eventoId);
        dto.UsuarioId.Should().Be(usuarioId);
        dto.AsientoId.Should().Be(asientoId);
        dto.Monto.Should().Be(monto);
        dto.CodigoQr.Should().Be(codigoQr);
        dto.Estado.Should().Be(EstadoEntrada.PendientePago);
        dto.FechaCompra.Should().Be(entrada.FechaCompra);
    }

    [Fact]
    public void ToEntradaCreadaDto_ConEntradaNula_DebeLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => EntradaMapper.ToEntradaCreadaDto(null!));
    }

    [Fact]
    public void ToCommand_ConCrearEntradaDtoCompleto_DebeMappearCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var monto = 175.50m;

        var dto = new CrearEntradaDto(eventoId, usuarioId, asientoId, null);

        // Act
        var command = EntradaMapper.ToCommand(dto);

        // Assert
        command.Should().NotBeNull();
        command.EventoId.Should().Be(eventoId);
        command.UsuarioId.Should().Be(usuarioId);
        command.AsientoId.Should().Be(asientoId);
        command.AsientoId.Should().Be(asientoId);
    }

    [Fact]
    public void ToCommand_ConCrearEntradaDtoGeneral_DebeMappearAsientoIdComoNull()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var monto = 125.00m;

        var dto = new CrearEntradaDto(eventoId, usuarioId, null, null);

        // Act
        var command = EntradaMapper.ToCommand(dto);

        // Assert
        command.Should().NotBeNull();
        command.AsientoId.Should().BeNull();
        command.EventoId.Should().Be(eventoId);
        command.UsuarioId.Should().Be(usuarioId);
        command.UsuarioId.Should().Be(usuarioId);
    }

    [Fact]
    public void ToCommand_ConDtoNulo_DebeLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => EntradaMapper.ToCommand(null!));
    }

    [Fact]
    public void ToDto_ConListaDeEntradas_DebeMappearTodasCorrectamente()
    {
        // Arrange
        var entrada1 = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100.00m, null, "TICKET-001");
        var entrada2 = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 200.00m, Guid.NewGuid(), "TICKET-002");
        var entrada3 = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 300.00m, null, "TICKET-003");
        entrada3.ConfirmarPago(); // Una entrada pagada

        var entradas = new List<Entrada> { entrada1, entrada2, entrada3 };

        // Act
        var dtos = EntradaMapper.ToDto(entradas).ToList();

        // Assert
        dtos.Should().HaveCount(3);

        var dto1 = dtos.First(x => x.Id == entrada1.Id);
        dto1.Monto.Should().Be(100.00m);
        dto1.AsientoId.Should().BeNull();
        dto1.Estado.Should().Be(EstadoEntrada.PendientePago);

        var dto2 = dtos.First(x => x.Id == entrada2.Id);
        dto2.Monto.Should().Be(200.00m);
        dto2.AsientoId.Should().NotBeNull();
        dto2.Estado.Should().Be(EstadoEntrada.PendientePago);

        var dto3 = dtos.First(x => x.Id == entrada3.Id);
        dto3.Monto.Should().Be(300.00m);
        dto3.Estado.Should().Be(EstadoEntrada.Pagada);
    }

    [Fact]
    public void ToDto_ConListaVacia_DebeRetornarListaVacia()
    {
        // Arrange
        var entradas = new List<Entrada>();

        // Act
        var dtos = EntradaMapper.ToDto(entradas);

        // Assert
        dtos.Should().NotBeNull();
        dtos.Should().BeEmpty();
    }

    [Fact]
    public void ToDto_ConListaNula_DebeLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => EntradaMapper.ToDto((IEnumerable<Entrada>)null!));
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(999.99)]
    [InlineData(999999.99)]
    public void ToDto_ConDiferentesMontos_DebeMappearCorrectamente(decimal monto)
    {
        // Arrange
        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), monto, null, "TICKET-TEST");

        // Act
        var dto = EntradaMapper.ToDto(entrada);

        // Assert
        dto.Monto.Should().Be(monto);
    }

    [Fact]
    public void ToDto_ConCodigoQrEspecial_DebeMappearCorrectamente()
    {
        // Arrange
        var codigosQr = new[]
        {
            "TICKET-ABC123-4567",
            "TICKET-XYZ999-0000",
            "TICKET-123ABC-DEF4",
            "TICKET-000000-9999"
        };

        foreach (var codigoQr in codigosQr)
        {
            var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100.00m, null, codigoQr);

            // Act
            var dto = EntradaMapper.ToDto(entrada);

            // Assert
            dto.CodigoQr.Should().Be(codigoQr);
        }
    }

    [Fact]
    public void ToDto_ConFechasVariadas_DebeMappearCorrectamente()
    {
        // Arrange
        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100.00m, null, "TICKET-DATE");
        var fechaOriginal = entrada.FechaCompra;

        // Act
        var dto = EntradaMapper.ToDto(entrada);

        // Assert
        dto.FechaCompra.Should().Be(fechaOriginal);
        dto.FechaCompra.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void ToDto_Y_ToCommand_RoundTrip_DebePreservarDatos()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var asientoId = Guid.NewGuid();
        var monto = 350.75m;

        var dtoOriginal = new CrearEntradaDto(eventoId, usuarioId, asientoId, null);

        // Act
        var command = EntradaMapper.ToCommand(dtoOriginal);

        // Assert - Verificar que los datos se preservan en el round trip
        command.EventoId.Should().Be(dtoOriginal.EventoId);
        command.UsuarioId.Should().Be(dtoOriginal.UsuarioId);
        command.AsientoId.Should().Be(dtoOriginal.AsientoId);
        command.AsientoId.Should().Be(dtoOriginal.AsientoId);
    }

    [Fact]
    public void ToDto_Y_ToEntradaCreadaDto_DebenTenerMismosDatos()
    {
        // Arrange
        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 400.00m, Guid.NewGuid(), "TICKET-COMPARE");

        // Act
        var entradaDto = EntradaMapper.ToDto(entrada);
        var entradaCreadaDto = EntradaMapper.ToEntradaCreadaDto(entrada);

        // Assert - Ambos DTOs deben tener los mismos datos
        entradaDto.Id.Should().Be(entradaCreadaDto.Id);
        entradaDto.EventoId.Should().Be(entradaCreadaDto.EventoId);
        entradaDto.UsuarioId.Should().Be(entradaCreadaDto.UsuarioId);
        entradaDto.AsientoId.Should().Be(entradaCreadaDto.AsientoId);
        entradaDto.Monto.Should().Be(entradaCreadaDto.Monto);
        entradaDto.CodigoQr.Should().Be(entradaCreadaDto.CodigoQr);
        entradaDto.Estado.Should().Be(entradaCreadaDto.Estado);
        entradaDto.FechaCompra.Should().Be(entradaCreadaDto.FechaCompra);
    }

    [Fact]
    public void ToDto_ConListaGrande_DebeProcessarEficientemente()
    {
        // Arrange
        var entradas = new List<Entrada>();
        for (int i = 0; i < 1000; i++)
        {
            var entrada = Entrada.Crear(
                Guid.NewGuid(), 
                Guid.NewGuid(), 
                100.00m + i, 
                i % 2 == 0 ? Guid.NewGuid() : null, 
                $"TICKET-{i:D6}-TEST");
            entradas.Add(entrada);
        }

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var dtos = EntradaMapper.ToDto(entradas).ToList();
        stopwatch.Stop();

        // Assert
        dtos.Should().HaveCount(1000);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Debe ser rápido
        
        // Verificar algunos elementos aleatorios
        var dto500 = dtos[500];
        dto500.Monto.Should().Be(600.00m); // 100 + 500
        dto500.CodigoQr.Should().Be("TICKET-000500-TEST");
    }
}