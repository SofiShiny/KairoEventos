using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Asientos.Infraestructura.Persistencia;

// Fábrica design-time para que dotnet-ef pueda crear el DbContext sin depender de Program.cs
public class AsientosDbContextFactory : IDesignTimeDbContextFactory<AsientosDbContext>
{
    public AsientosDbContext CreateDbContext(string[] args)
    {
        // Permite override por variable de entorno si existe, sino usa valor por defecto local.
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5433"; // puerto host del compose
        var db   = Environment.GetEnvironmentVariable("POSTGRES_DB_ASIENTOS") ?? "asientosdb";
        var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
        var pass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";
        var cs = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";

        var opts = new DbContextOptionsBuilder<AsientosDbContext>()
            .UseNpgsql(cs)
            .Options;
        return new AsientosDbContext(opts);
    }
}
