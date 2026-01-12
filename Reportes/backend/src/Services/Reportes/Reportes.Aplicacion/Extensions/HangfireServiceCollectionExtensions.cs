using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reportes.Aplicacion.Extensions;

/// <summary>
/// Extension methods for configuring Hangfire services
/// </summary>
public static class HangfireServiceCollectionExtensions
{
    /// <summary>
    /// Configures Hangfire with MongoDB storage
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection ConfigurarHangfire(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var enableHangfire = configuration.GetValue<bool>("Hangfire:Enabled", true);
        
        if (enableHangfire)
        {
            var connectionString = GetHangfireConnectionString(configuration);
            
            ConfigureHangfireStorage(services, connectionString);
            ConfigureHangfireServer(services);
        }

        return services;
    }

    /// <summary>
    /// Gets the MongoDB connection string for Hangfire from environment variables or configuration
    /// </summary>
    private static string GetHangfireConnectionString(IConfiguration configuration)
    {
        var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
            ?? configuration["MongoDbSettings:ConnectionString"]
            ?? "mongodb://localhost:27017";

        var mongoDatabase = Environment.GetEnvironmentVariable("MONGODB_DATABASE")
            ?? configuration["MongoDbSettings:DatabaseName"]
            ?? "reportes_db";

        return $"{mongoConnectionString}/{mongoDatabase}";
    }

    /// <summary>
    /// Configures Hangfire storage with MongoDB
    /// </summary>
    private static void ConfigureHangfireStorage(IServiceCollection services, string connectionString)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseMongoStorage(connectionString, CreateMongoStorageOptions()));
    }

    /// <summary>
    /// Creates MongoDB storage options for Hangfire
    /// </summary>
    private static MongoStorageOptions CreateMongoStorageOptions()
    {
        return new MongoStorageOptions
        {
            MigrationOptions = new MongoMigrationOptions
            {
                MigrationStrategy = new MigrateMongoMigrationStrategy(),
                BackupStrategy = new CollectionMongoBackupStrategy()
            },
            Prefix = "hangfire",
            CheckConnection = false, // Disable connection check for tests
            CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
        };
    }

    /// <summary>
    /// Configures Hangfire server options
    /// </summary>
    private static void ConfigureHangfireServer(IServiceCollection services)
    {
        services.AddHangfireServer(options =>
        {
            options.ServerName = "Reportes-Server";
            options.WorkerCount = 1;
        });
    }
}