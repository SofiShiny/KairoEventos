/*using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;
using Entradas.Infraestructura.Persistencia;
using Entradas.Dominio.Entidades;
using Entradas.Dominio.Enums;

namespace Entradas.Pruebas.Integracion;

/// <summary>
/// Pruebas de integración de base de datos con TestContainers PostgreSQL
/// Valida operaciones CRUD reales y transacciones distribuidas
/// </summary>
public class DatabaseIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private EntradasDbContext? _context;
    private string? _connectionString;

    public async Task InitializeAsync()
    {
        // Crear contenedor PostgreSQL
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("entradas_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _container.StartAsync();

        _connectionString = _container.GetConnectionString();

        // Configurar DbContext con la conexión del contenedor
        var options = new DbContextOptionsBuilder<EntradasDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        _context = new EntradasDbContext(options);

        // Crear esquema de base de datos
        await _context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }

        if (_container != null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }

    [Fact]
    public async Task Database_DebeConectarseCorrectamente()
    {
        // Act
        var canConnect = await _context!.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task Database_DebeCrearEsquemaCorrectamente()
    {
        // Act
        var tables = await _context!.Database.SqlQuery<string>(
            $"SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'")
            .ToListAsync();

        // Assert
        tables.Should().NotBeEmpty();
        tables.Should().Contain("entradas");
    }

    [Fact]
    public async Task CRUD_CrearEntrada_DebeGuardarCorrectamente()
    {
        // Arrange
        var entrada = Entrada.Crear(
            eventoId: Guid.NewGuid(),
            usuarioId: Guid.NewGuid(),
            monto: 150.75m,
            asientoId: Guid.NewGuid(),
            codigoQr: "CRUD-CREATE-001"
        );

        // Act
        _context!.Entradas.Add(entrada);
        await _context.SaveChangesAsync();

        // Assert
        var entradaGuardada = await _context.Entradas
            .FirstOrDefaultAsync(e => e.Id == entrada.Id);

        entradaGuardada.Should().NotBeNull();
        entradaGuardada!.CodigoQr.Should().Be("CRUD-CREATE-001");
        entradaGuardada.Monto.Should().Be(150.75m);
        entradaGuardada.Estado.Should().Be(EstadoEntrada.PendientePago);
    }

    [Fact]
    public async Task CRUD_LeerEntrada_DebeRetornarCorrectamente()
    {
        // Arrange
        var entrada = Entrada.Crear(
            Guid.NewGuid(), Guid.NewGuid(), 200m, Guid.NewGuid(), "CRUD-READ-001"
        );
        _context!.Entradas.Add(entrada);
        await _context.SaveChangesAsync();

        // Act
        var entradaLeida = await _context.Entradas
            .FirstOrDefaultAsync(e => e.Id == entrada.Id);

        // Assert
        entradaLeida.Should().NotBeNull();
        entradaLeida!.Id.Should().Be(entrada.Id);
        entradaLeida.CodigoQr.Should().Be("CRUD-READ-001");
    }

    [Fact]
    public async Task CRUD_ActualizarEntrada_DebeModificarCorrectamente()
    {
        // Arrange
        var entrada = Entrada.Crear(
            Guid.NewGuid(), Guid.NewGuid(), 250m, Guid.NewGuid(), "CRUD-UPDATE-001"
        );
        _context!.Entradas.Add(entrada);
        await _context.SaveChangesAsync();

        // Act
        entrada.ConfirmarPago();
        _context.Entradas.Update(entrada);
        await _context.SaveChangesAsync();

        // Assert
        var entradaActualizada = await _context.Entradas
            .FirstOrDefaultAsync(e => e.Id == entrada.Id);

        entradaActualizada.Should().NotBeNull();
        entradaActualizada!.Estado.Should().Be(EstadoEntrada.Pagada);
    }

    [Fact]
    public async Task CRUD_EliminarEntrada_DebeRemoverCorrectamente()
    {
        // Arrange
        var entrada = Entrada.Crear(
            Guid.NewGuid(), Guid.NewGuid(), 300m, Guid.NewGuid(), "CRUD-DELETE-001"
        );
        _context!.Entradas.Add(entrada);
        await _context.SaveChangesAsync();

        var entradaId = entrada.Id;

        // Act
        _context.Entradas.Remove(entrada);
        await _context.SaveChangesAsync();

        // Assert
        var entradaEliminada = await _context.Entradas
            .FirstOrDefaultAsync(e => e.Id == entradaId);

        entradaEliminada.Should().BeNull();
    }

    [Fact]
    public async Task Transacciones_Commit_DebeGuardarCambios()
    {
        // Arrange
        var entrada = Entrada.Crear(
            Guid.NewGuid(), Guid.NewGuid(), 350m, Guid.NewGuid(), "TRANS-COMMIT-001"
        );

        // Act
        using (var transaction = await _context!.Database.BeginTransactionAsync())
        {
            _context.Entradas.Add(entrada);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        // Assert
        var entradaGuardada = await _context.Entradas
            .FirstOrDefaultAsync(e => e.Id == entrada.Id);

        entradaGuardada.Should().NotBeNull();
    }

    [Fact]
    public async Task Transacciones_Rollback_NoDebeGuardarCambios()
    {
        // Arrange
        var entrada = Entrada.Crear(
            Guid.NewGuid(), Guid.NewGuid(), 400m, Guid.NewGuid(), "TRANS-ROLLBACK-001"
        );

        var entradaId = entrada.Id;

        // Act
        using (var transaction = await _context!.Database.BeginTransactionAsync())
        {
            _context.Entradas.Add(entrada);
            await _context.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        // Assert
        var entradaGuardada = await _context.Entradas
            .FirstOrDefaultAsync(e => e.Id == entradaId);

        entradaGuardada.Should().BeNull();
    }

    [Fact]
    public async Task Transacciones_MultipleOperaciones_DebeSerAtomica()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var entrada1 = Entrada.Crear(Guid.NewGuid(), usuarioId, 100m, Guid.NewGuid(), "MULTI-1");
        var entrada2 = Entrada.Crear(Guid.NewGuid(), usuarioId, 200m, Guid.NewGuid(), "MULTI-2");

        // Act
        using (var transaction = await _context!.Database.BeginTransactionAsync())
        {
            _context.Entradas.Add(entrada1);
            _context.Entradas.Add(entrada2);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        // Assert
        var entradasGuardadas = await _context.Entradas
            .Where(e => e.UsuarioId == usuarioId)
            .ToListAsync();

        entradasGuardadas.Should().HaveCount(2);
    }

    [Fact]
    public async Task Transacciones_ErrorEnMedio_DebeHacerRollback()
    {
        // Arrange
        var entrada1 = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid(), "ERROR-1");
        var entrada2 = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 200m, Guid.NewGuid(), "ERROR-2");

        // Act & Assert
        using (var transaction = await _context!.Database.BeginTransactionAsync())
        {
            _context.Entradas.Add(entrada1);
            await _context.SaveChangesAsync();

            // Simular error
            try
            {
                _context.Entradas.Add(entrada2);
                // Forzar un error
                throw new InvalidOperationException("Error simulado");
            }
            catch
            {
                await transaction.RollbackAsync();
            }
        }

        // Verificar que no se guardó nada
        var count = await _context.Entradas.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task Consultas_FiltrarPorUsuario_DebeRetornarCorrectamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var otroUsuarioId = Guid.NewGuid();

        var entradas = new List<Entrada>
        {
            Entrada.Crear(Guid.NewGuid(), usuarioId, 100m, Guid.NewGuid(), "USER-FILTER-1"),
            Entrada.Crear(Guid.NewGuid(), usuarioId, 200m, Guid.NewGuid(), "USER-FILTER-2"),
            Entrada.Crear(Guid.NewGuid(), otroUsuarioId, 300m, Guid.NewGuid(), "OTHER-USER")
        };

        _context!.Entradas.AddRange(entradas);
        await _context.SaveChangesAsync();

        // Act
        var entradasUsuario = await _context.Entradas
            .Where(e => e.UsuarioId == usuarioId)
            .ToListAsync();

        // Assert
        entradasUsuario.Should().HaveCount(2);
        entradasUsuario.Should().AllSatisfy(e => e.UsuarioId.Should().Be(usuarioId));
    }

    [Fact]
    public async Task Consultas_FiltrarPorEvento_DebeRetornarCorrectamente()
    {
        // Arrange
        var eventoId = Guid.NewGuid();
        var otroEventoId = Guid.NewGuid();

        var entradas = new List<Entrada>
        {
            Entrada.Crear(eventoId, Guid.NewGuid(), 100m, Guid.NewGuid(), "EVENT-FILTER-1"),
            Entrada.Crear(eventoId, Guid.NewGuid(), 200m, Guid.NewGuid(), "EVENT-FILTER-2"),
            Entrada.Crear(otroEventoId, Guid.NewGuid(), 300m, Guid.NewGuid(), "OTHER-EVENT")
        };

        _context!.Entradas.AddRange(entradas);
        await _context.SaveChangesAsync();

        // Act
        var entradasEvento = await _context.Entradas
            .Where(e => e.EventoId == eventoId)
            .ToListAsync();

        // Assert
        entradasEvento.Should().HaveCount(2);
        entradasEvento.Should().AllSatisfy(e => e.EventoId.Should().Be(eventoId));
    }

    [Fact]
    public async Task Consultas_FiltrarPorEstado_DebeRetornarCorrectamente()
    {
        // Arrange
        var entrada1 = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid(), "STATE-1");
        var entrada2 = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 200m, Guid.NewGuid(), "STATE-2");
        var entrada3 = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 300m, Guid.NewGuid(), "STATE-3");

        entrada2.ConfirmarPago();
        entrada3.ConfirmarPago();

        _context!.Entradas.AddRange(entrada1, entrada2, entrada3);
        await _context.SaveChangesAsync();

        // Act
        var entradasPagadas = await _context.Entradas
            .Where(e => e.Estado == EstadoEntrada.Pagada)
            .ToListAsync();

        // Assert
        entradasPagadas.Should().HaveCount(2);
        entradasPagadas.Should().AllSatisfy(e => e.Estado.Should().Be(EstadoEntrada.Pagada));
    }

    [Fact]
    public async Task Consultas_BuscarPorCodigoQr_DebeRetornarCorrectamente()
    {
        // Arrange
        var codigoQr = "UNIQUE-QR-CODE-123";
        var entrada = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid(), codigoQr);

        _context!.Entradas.Add(entrada);
        await _context.SaveChangesAsync();

        // Act
        var entradaEncontrada = await _context.Entradas
            .FirstOrDefaultAsync(e => e.CodigoQr == codigoQr);

        // Assert
        entradaEncontrada.Should().NotBeNull();
        entradaEncontrada!.CodigoQr.Should().Be(codigoQr);
    }

    [Fact]
    public async Task Indices_CodigoQrUnico_DebeEnforzarUnicidad()
    {
        // Arrange
        var codigoQr = "DUPLICATE-QR-CODE";
        var entrada1 = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid(), codigoQr);
        var entrada2 = Entrada.Crear(Guid.NewGuid(), Guid.NewGuid(), 200m, Guid.NewGuid(), codigoQr);

        _context!.Entradas.Add(entrada1);
        await _context.SaveChangesAsync();

        // Act & Assert
        _context.Entradas.Add(entrada2);
        
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            async () => await _context.SaveChangesAsync()
        );

        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task Performance_InsertarMultiplesEntradas_DebeSerRapido()
    {
        // Arrange
        var entradas = Enumerable.Range(1, 100)
            .Select(i => Entrada.Crear(
                Guid.NewGuid(),
                Guid.NewGuid(),
                100m + i,
                Guid.NewGuid(),
                $"PERF-{i:D4}"
            ))
            .ToList();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _context!.Entradas.AddRange(entradas);
        await _context.SaveChangesAsync();
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
        
        var count = await _context.Entradas.CountAsync();
        count.Should().Be(100);
    }

    [Fact]
    public async Task Performance_ConsultarMultiplesEntradas_DebeSerRapido()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var entradas = Enumerable.Range(1, 50)
            .Select(i => Entrada.Crear(
                Guid.NewGuid(),
                usuarioId,
                100m + i,
                Guid.NewGuid(),
                $"QUERY-{i:D4}"
            ))
            .ToList();

        _context!.Entradas.AddRange(entradas);
        await _context.SaveChangesAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var entradasConsultadas = await _context.Entradas
            .Where(e => e.UsuarioId == usuarioId)
            .ToListAsync();
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
        entradasConsultadas.Should().HaveCount(50);
    }
}
*/