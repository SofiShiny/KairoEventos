using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Eventos.Aplicacion;

namespace Eventos.Pruebas.Aplicacion;

public class ServiceCollectionExtensionsAppTests
{
 [Fact]
 public void AddEventAplicacionServices_RegistraMediatR_Y_Validators()
 {
 var services = new ServiceCollection();
 services.AddEventAplicacionServices();
 var provider = services.BuildServiceProvider();
 provider.GetService<MediatR.IMediator>().Should().NotBeNull();
 // Uno de los validadores del ensamblado
 provider.GetService<FluentValidation.IValidator<Eventos.Aplicacion.DTOs.EventoCreateDto>>().Should().NotBeNull();
 }
}
