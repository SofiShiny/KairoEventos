using BloquesConstruccion.Dominio;
using Eventos.Dominio.EventosDeDominio;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio.EventosDominio;

// ========== Pruebas de AsistenteRegistradoEventoDominioTests.cs ==========

public class AsistenteRegistradoEventoDominioTests
{
 [Fact]
 public void Constructor_DeberiaInicializarPropiedades()
 {
 // Preparar
 var eventoId = Guid.NewGuid();
 var usuarioId = "usuario-001";
 var nombre = "Creonte Dioniso Lara Wilson";

 // Actuar
 var domainEvent = new AsistenteRegistradoEventoDominio(eventoId, usuarioId, nombre);

 // Comprobar
 domainEvent.EventoId.Should().Be(eventoId);
 domainEvent.UsuarioId.Should().Be(usuarioId);
 domainEvent.NombreUsuario.Should().Be(nombre);
 domainEvent.OcurrioEn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
 }

 [Fact]
 public void AsistenteRegistradoEventoDominio_DeberiaDerivarDeEventoDominio()
 {
 // Actuar
 var domainEvent = new AsistenteRegistradoEventoDominio(Guid.NewGuid(), "usuario-001", "Creonte Dioniso Lara Wilson");

 // Comprobar
 domainEvent.Should().BeAssignableTo<EventoDominio>();
 }

 [Theory]
 [InlineData("", "")]
 [InlineData(null, null)]
 [InlineData("usuario-001", "")]
 [InlineData("", "NombreValido")]
 public void Constructor_DeberiaAceptarCadenasNulasOVacias(string? usuarioId, string? nombre)
 {
 // Actuar
 var domainEvent = new AsistenteRegistradoEventoDominio(Guid.NewGuid(), usuarioId!, nombre!);

 // Comprobar
 domainEvent.UsuarioId.Should().Be(usuarioId);
 domainEvent.NombreUsuario.Should().Be(nombre);
 }

 [Fact]
 public void Constructor_DeberiaAceptarGuidVacio()
 {
 // Actuar
 var domainEvent = new AsistenteRegistradoEventoDominio(Guid.Empty, "usuario-001", "Creonte Dioniso Lara Wilson");

 // Comprobar
 domainEvent.EventoId.Should().Be(Guid.Empty);
 }
}
