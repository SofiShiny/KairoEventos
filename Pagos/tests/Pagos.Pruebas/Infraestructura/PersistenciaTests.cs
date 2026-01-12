using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pagos.Dominio.Entidades;
using Pagos.Dominio.Modelos;
using Pagos.Infraestructura.Persistencia;
using Xunit;

namespace Pagos.Pruebas.Infraestructura;

public class PersistenciaTests
{
    private PagosDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<PagosDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new PagosDbContext(options);
    }

    [Fact]
    public async Task RepositorioTransacciones_OperacionesBasicas_FuncionanCorrectamente()
    {
        // Arrange
        var context = GetDbContext("RepositorioTest");
        var repo = new RepositorioTransacciones(context);
        var tx = new Transaccion
        {
            Id = Guid.NewGuid(),
            OrdenId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            Monto = 100,
            Estado = EstadoTransaccion.Pendiente,
            FechaCreacion = DateTime.UtcNow,
            TarjetaMascara = "**** 1234"
        };

        // Act - Agregar
        await repo.AgregarAsync(tx);
        
        // Assert - Obtener por ID
        var obtenido = await repo.ObtenerPorIdAsync(tx.Id);
        obtenido.Should().NotBeNull();
        obtenido!.Monto.Should().Be(100);

        // Act - Actualizar
        obtenido.Aprobar("url-test");
        await repo.ActualizarAsync(obtenido);

        // Assert - Verificar actualización
        var actualizado = await repo.ObtenerPorIdAsync(tx.Id);
        actualizado!.Estado.Should().Be(EstadoTransaccion.Aprobado);
        actualizado.UrlFactura.Should().Be("url-test");

        // Act - Obtener todas
        var todas = await repo.ObtenerTodasAsync();
        todas.Should().HaveCount(1);
    }

    [Fact]
    public void PagosDbContext_OnModelCreating_ConfiguraModeloCorrectamente()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PagosDbContext>()
            .UseInMemoryDatabase(databaseName: "ModelTest")
            .Options;
        var context = new PagosDbContext(options);

        // Act
        var model = context.Model;
        var entityType = model.FindEntityType(typeof(Transaccion));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.FindProperty("Monto")!.GetPrecision().Should().Be(18);
        entityType.FindProperty("Monto")!.GetScale().Should().Be(2);
        entityType.FindProperty("TarjetaMascara")!.GetMaxLength().Should().Be(20);
    }
}
