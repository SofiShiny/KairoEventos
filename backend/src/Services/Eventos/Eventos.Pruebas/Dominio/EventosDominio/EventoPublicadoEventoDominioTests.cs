using BloquesConstruccion.Dominio;
using Eventos.Dominio.EventosDeDominio;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio.EventosDominio;

// ========== Pruebas de EventoPublicadoEventoDominioTests.cs ==========

public class EventoPublicadoEventoDominioTests
{
 [Fact]
 public void Constructor_DeberiaInicializarPropiedades()
 {
 // Preparar
 var eventoId = Guid.NewGuid();
 var titulo = "Taller de Arte";
 var fechaInicio = DateTime.UtcNow.AddDays(30);

 // Actuar
 var domainEvent = new EventoPublicadoEventoDominio(eventoId, titulo, fechaInicio);

 // Comprobar
 domainEvent.EventoId.Should().Be(eventoId);
 domainEvent.TituloEvento.Should().Be(titulo);
 domainEvent.FechaInicio.Should().Be(fechaInicio);
 domainEvent.OcurrioEn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
 }

 [Fact]
 public void EventoPublicadoEventoDominio_DeberiaDerivarDeEventoDominio()
 {
 // Actuar
 var domainEvent = new EventoPublicadoEventoDominio(Guid.NewGuid(), "Taller de Arte", DateTime.UtcNow);

 // Comprobar
 domainEvent.Should().BeAssignableTo<EventoDominio>();
 }

 [Theory]
 [InlineData("")]
 [InlineData(null)]
 public void Constructor_DeberiaAceptarTituloNuloOVacio(string? titulo)
 {
 // Actuar
 var domainEvent = new EventoPublicadoEventoDominio(Guid.NewGuid(), titulo!, DateTime.UtcNow);

 // Comprobar
 domainEvent.TituloEvento.Should().Be(titulo);
 }

 [Fact]
 public void Constructor_DeberiaAceptarGuidVacio()
 {
 // Actuar
 var domainEvent = new EventoPublicadoEventoDominio(Guid.Empty, "Taller de Arte", DateTime.UtcNow);

 // Comprobar
 domainEvent.EventoId.Should().Be(Guid.Empty);
 }

 [Fact]
 public void Constructor_DeberiaAceptarFechasPasadasYFuturas()
 {
 // Preparar
 var fechaPasada = DateTime.UtcNow.AddDays(-5);
 var fechaFutura = DateTime.UtcNow.AddDays(5);

 // Actuar
 var eventoPasado = new EventoPublicadoEventoDominio(Guid.NewGuid(), "Taller de Arte", fechaPasada);
 var eventoFuturo = new EventoPublicadoEventoDominio(Guid.NewGuid(), "Taller de Arte", fechaFutura);

 // Comprobar
 eventoPasado.FechaInicio.Should().Be(fechaPasada);
 eventoFuturo.FechaInicio.Should().Be(fechaFutura);
 }
}
