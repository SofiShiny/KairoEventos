using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Xunit;
using BloquesConstruccion.Aplicacion.Comportamientos;

namespace Eventos.Pruebas.Shared.BloquesConstruccion.Aplicacion.Comportamientos;

public class ValidationBehaviorTests
{
 private class Ping : IRequest<string>
 {
 public string Msg { get; set; } = string.Empty;
 }
 private class PingValidator : AbstractValidator<Ping>
 {
 public PingValidator()
 {
 RuleFor(x => x.Msg).NotEmpty();
 }
 }

 [Fact]
 public async Task Handle_SinValidadores_ContinuaConSiguiente()
 {
 var behavior = new ValidationBehavior<Ping, string>(Array.Empty<IValidator<Ping>>());
 var called = false;
 Task<string> Next() { called = true; return Task.FromResult("ok"); }
 var res = await behavior.Handle(new Ping{ Msg=""}, Next, CancellationToken.None);
 called.Should().BeTrue();
 res.Should().Be("ok");
 }

 [Fact]
 public async Task Handle_ConErroresDeValidacion_LanzaValidationException()
 {
 var behavior = new ValidationBehavior<Ping, string>(new IValidator<Ping>[] { new PingValidator() });
 Task<string> Next() => Task.FromResult("ok");
 var act = async () => await behavior.Handle(new Ping{ Msg=""}, Next, CancellationToken.None);
 await act.Should().ThrowAsync<ValidationException>();
 }

 [Fact]
 public async Task Handle_ValidacionExitosa_Continua()
 {
 var behavior = new ValidationBehavior<Ping, string>(new IValidator<Ping>[] { new PingValidator() });
 Task<string> Next() => Task.FromResult("ok");
 var res = await behavior.Handle(new Ping{ Msg="hola"}, Next, CancellationToken.None);
 res.Should().Be("ok");
 }
}
