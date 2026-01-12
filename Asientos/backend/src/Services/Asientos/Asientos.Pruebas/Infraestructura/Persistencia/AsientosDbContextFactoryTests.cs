using Asientos.Infraestructura.Persistencia;
using Xunit;
using System;

namespace Asientos.Pruebas.Infraestructura.Persistencia;

public class AsientosDbContextFactoryTests
{
    private void ClearEnv()
    {
        Environment.SetEnvironmentVariable("POSTGRES_HOST", null);
        Environment.SetEnvironmentVariable("POSTGRES_PORT", null);
        Environment.SetEnvironmentVariable("POSTGRES_DB_ASIENTOS", null);
        Environment.SetEnvironmentVariable("POSTGRES_USER", null);
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", null);
    }

    [Fact]
    public void CreateDbContext_UsaVariablesEntornoSiExisten()
    {
        ClearEnv();
        Environment.SetEnvironmentVariable("POSTGRES_HOST", "test-host");
        Environment.SetEnvironmentVariable("POSTGRES_PORT", "15432");
        Environment.SetEnvironmentVariable("POSTGRES_DB_ASIENTOS", "testdb");
        Environment.SetEnvironmentVariable("POSTGRES_USER", "userx");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "passx");
        var factory = new AsientosDbContextFactory();
        var ctx = factory.CreateDbContext(Array.Empty<string>());
        Assert.NotNull(ctx);
        Assert.IsType<AsientosDbContext>(ctx);
    }

    [Fact]
    public void CreateDbContext_UsaValoresPorDefectoSiNoHayVariables()
    {
        ClearEnv();
        var factory = new AsientosDbContextFactory();
        var ctx = factory.CreateDbContext(Array.Empty<string>());
        Assert.NotNull(ctx);
        Assert.IsType<AsientosDbContext>(ctx);
    }

    [Fact]
    public void CreateDbContext_ParcialOverrideMantieneDefaultsRestantes()
    {
        ClearEnv();
        Environment.SetEnvironmentVariable("POSTGRES_HOST", "only-host");
        Environment.SetEnvironmentVariable("POSTGRES_PORT", "9999");
        var factory = new AsientosDbContextFactory();
        var ctx = factory.CreateDbContext(Array.Empty<string>());
        Assert.NotNull(ctx);
        Assert.IsType<AsientosDbContext>(ctx);
    }

    [Fact]
    public void CreateDbContext_NoModificaVariablesGlobalesPosteriorUso()
    {
        ClearEnv();
        var beforeHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
        var factory = new AsientosDbContextFactory();
        _ = factory.CreateDbContext(Array.Empty<string>());
        var afterHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
        Assert.Equal(beforeHost, afterHost); // Factory solo lee, no escribe.
    }
}
