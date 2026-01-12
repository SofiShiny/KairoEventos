using BloquesConstruccion.Dominio;
using Eventos.Dominio.EventosDeDominio;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio.EventosDominio;

// ========== Pruebas de EventoCanceladoEventoDominioTests.cs ==========

public class EventoCanceladoEventoDominioTests
{
 [Fact]
 public void Constructor_DeberiaInicializarPropiedades()
 {
 // Preparar
 var eventoId = Guid.NewGuid();
 var titulo = "Taller de Arte";

 // Actuar
 var domainEvent = new EventoCanceladoEventoDominio(eventoId, titulo);

 // Comprobar
 domainEvent.EventoId.Should().Be(eventoId);
 domainEvent.TituloEvento.Should().Be(titulo);
 domainEvent.OcurrioEn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
 }

 [Fact]
 public void EventoCanceladoEventoDominio_DeberiaDerivarDeEventoDominio()
 {
 // Actuar
 var domainEvent = new EventoCanceladoEventoDominio(Guid.NewGuid(), "Taller de Arte");

 // Comprobar
 domainEvent.Should().BeAssignableTo<EventoDominio>();
 }

 [Theory]
 [InlineData("")]
 [InlineData(null)]
 public void Constructor_DeberiaAceptarTituloNuloOVacio(string? titulo)
 {
 // Actuar
 var domainEvent = new EventoCanceladoEventoDominio(Guid.NewGuid(), titulo!);

 // Comprobar
 domainEvent.TituloEvento.Should().Be(titulo);
 }

 [Fact]
 public void Constructor_DeberiaAceptarGuidVacio()
 {
 // Actuar
 var domainEvent = new EventoCanceladoEventoDominio(Guid.Empty, "Taller de Arte");

 // Comprobar
 domainEvent.EventoId.Should().Be(Guid.Empty);
 }
}
