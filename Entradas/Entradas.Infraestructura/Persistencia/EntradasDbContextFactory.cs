using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Entradas.Infraestructura.Persistencia;

/// <summary>
/// Factory para crear el DbContext durante las migraciones
/// </summary>
public class EntradasDbContextFactory : IDesignTimeDbContextFactory<EntradasDbContext>
{
    public EntradasDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EntradasDbContext>();
        
        // Configuración para migraciones - usar connection string por defecto
        var connectionString = "Host=localhost;Database=entradas_dev;Username=postgres;Password=postgres";
        
        // Intentar leer de configuración si existe
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
                
            var configConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(configConnectionString))
            {
                connectionString = configConnectionString;
            }
        }
        catch
        {
            // Si no se puede leer la configuración, usar la por defecto
        }
        
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsHistoryTable("__EFMigrationsHistory", "entradas");
        });
        
        return new EntradasDbContext(optionsBuilder.Options);
    }
}