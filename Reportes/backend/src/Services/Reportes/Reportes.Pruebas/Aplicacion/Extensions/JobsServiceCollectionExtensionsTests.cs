using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Reportes.Aplicacion.Extensions;
using Reportes.Aplicacion.Jobs;
using Reportes.Dominio.Repositorios;
using Xunit;

namespace Reportes.Pruebas.Aplicacion.Extensions;

public class JobsServiceCollectionExtensionsTests
{
    private readonly IServiceCollection _services;

    public JobsServiceCollectionExtensionsTests()
    {
        _services = new ServiceCollection();
        AddMockDependencies(_services);
    }

    [Fact]
    public void ConfigurarJobs_RegistraJobGenerarReportesConsolidados()
    {
        // Act
        _services.ConfigurarJobs();
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var job = serviceProvider.GetService<JobGenerarReportesConsolidados>();
        job.Should().NotBeNull();
    }

    [Fact]
    public void ConfigurarJobs_RegistraJobComoScoped()
    {
        // Act
        _services.ConfigurarJobs();

        // Assert
        var jobDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(JobGenerarReportesConsolidados));
        jobDescriptor.Should().NotBeNull();
        jobDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void ConfigurarJobs_RetornaServiceCollection_ParaChaining()
    {
        // Act
        var result = _services.ConfigurarJobs();

        // Assert
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void ConfigurarJobs_PuedeResolverJobMultiplesVeces()
    {
        // Arrange
        _services.ConfigurarJobs();
        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var job1 = serviceProvider.GetService<JobGenerarReportesConsolidados>();
        var job2 = serviceProvider.GetService<JobGenerarReportesConsolidados>();

        // Assert
        job1.Should().NotBeNull();
        job2.Should().NotBeNull();
        // Como es Scoped, en el mismo scope debería ser la misma instancia
        job1.Should().BeSameAs(job2);
    }

    [Fact]
    public void ConfigurarJobs_ConNuevoScope_CreaInstanciaDiferente()
    {
        // Arrange
        _services.ConfigurarJobs();
        var serviceProvider = _services.BuildServiceProvider();

        // Act
        JobGenerarReportesConsolidados job1, job2;
        
        using (var scope1 = serviceProvider.CreateScope())
        {
            job1 = scope1.ServiceProvider.GetRequiredService<JobGenerarReportesConsolidados>();
        }
        
        using (var scope2 = serviceProvider.CreateScope())
        {
            job2 = scope2.ServiceProvider.GetRequiredService<JobGenerarReportesConsolidados>();
        }

        // Assert
        job1.Should().NotBeNull();
        job2.Should().NotBeNull();
        job1.Should().NotBeSameAs(job2);
    }

    private static void AddMockDependencies(IServiceCollection services)
    {
        // Add mock dependencies that the jobs need
        var mockRepository = new Mock<IRepositorioReportesLectura>();
        services.AddSingleton(mockRepository.Object);
        
        // Add logging
        services.AddLogging();
    }
}