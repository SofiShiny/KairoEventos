using System;
using System.Linq;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.ObjetosDeValor;
using FluentAssertions;
using Xunit;
using Eventos.Dominio.Enumeraciones;
using Eventos.Dominio.EventosDeDominio;

namespace Eventos.Pruebas.Dominio;

public class EventoAggregateTests
{
 private readonly Ubicacion _ubicBase;
 private readonly DateTime _inicioBase;
 private readonly DateTime _finBase;

 public EventoAggregateTests()
 {
 _ubicBase = new Ubicacion("Lugar", "Direccion", "Ciudad", "Region", "0000", "Pais");
 _inicioBase = DateTime.UtcNow.AddDays(5);
 _finBase = _inicioBase.AddDays(1);
 }

 private Evento Build(int max =10, DateTime? inicio=null, DateTime? fin=null, string titulo="Titulo", string desc="Descripcion", string org="organizador-001")
 => new(titulo, desc, _ubicBase, inicio ?? _inicioBase, fin ?? _finBase, max, org);

 [Fact]
 public void Constructor_ParametrosValidos_CreaEnBorrador()
 {
 var ev = Build();
 ev.Estado.Should().Be(EstadoEvento.Borrador);
 ev.Id.Should().NotBe(Guid.Empty);
 }

 [Fact]
 public void Constructor_TituloVacio_LanzaExcepcion()
 {
 Action act = () => Build(titulo:"", inicio:DateTime.UtcNow.AddDays(2), fin:DateTime.UtcNow.AddDays(3));
 act.Should().Throw<ArgumentException>();
 }

 [Fact]
 public void Constructor_DescripcionVacia_LanzaExcepcion()
 {
 Action act = () => Build(desc:"", inicio:DateTime.UtcNow.AddDays(2), fin:DateTime.UtcNow.AddDays(3));
 act.Should().Throw<ArgumentException>();
 }

 [Fact]
 public void Constructor_UbicacionNula_LanzaExcepcion()
 {
 Action act = () => new Evento("Titulo", "Descripcion", null!, DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(3),10, "org");
 act.Should().Throw<ArgumentNullException>();
 }

 [Fact]
 public void Constructor_FechaInicioPasada_LanzaExcepcion()
 {
 Action act = () => new Evento("Titulo", "Descripcion", _ubicBase, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddDays(1),10, "org");
 act.Should().Throw<ArgumentException>();
 }

 [Fact]
 public void Constructor_FechaFinAnteriorALaInicio_LanzaExcepcion()
 {
 var inicio = DateTime.UtcNow.AddDays(2);
 Action act = () => new Evento("Titulo", "Descripcion", _ubicBase, inicio, inicio,10, "org");
 act.Should().Throw<ArgumentException>();
 }

 [Fact]
 public void Constructor_MaximoAsistentesCero_LanzaExcepcion()
 {
 Action act = () => new Evento("Titulo", "Descripcion", _ubicBase, DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(3),0, "org");
 act.Should().Throw<ArgumentException>();
 }

 [Fact]
 public void Constructor_OrganizadorVacio_LanzaExcepcion()
 {
 Action act = () => new Evento("Titulo", "Descripcion", _ubicBase, DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(3),10, "");
 act.Should().Throw<ArgumentException>();
 }

 [Fact]
 public void Publicar_DesdeBorrador_GeneraEventoDominio()
 {
 var ev = Build();
 ev.Publicar();
 ev.Estado.Should().Be(EstadoEvento.Publicado);
 ev.EventosDominio.Should().ContainSingle().Which.Should().BeOfType<EventoPublicadoEventoDominio>();
 }

 [Fact]
 public void Publicar_NoEnBorrador_LanzaExcepcion()
 {
 var ev = Build();
 ev.Publicar();
 Action act = () => ev.Publicar();
 act.Should().Throw<InvalidOperationException>();
 }

 [Fact]
 public void Cancelar_Publicado_GeneraEventoDominio()
 {
 var ev = Build();
 ev.Publicar();
 ev.Cancelar();
 ev.Estado.Should().Be(EstadoEvento.Cancelado);
 ev.EventosDominio.Should().Contain(e => e is EventoCanceladoEventoDominio);
 }

 [Fact]
 public void RegistrarAsistente_NoPublicado_LanzaExcepcion()
 {
 var ev = Build();
 Action act = () => ev.RegistrarAsistente("u1", "Nombre", "correo@demo.com");
 act.Should().Throw<InvalidOperationException>().WithMessage("*no publicado*");
 }

 [Fact]
 public void RegistrarAsistente_Publicado_GeneraEventoDominioYIncrementaConteo()
 {
 var ev = Build();
 ev.Publicar();
 ev.RegistrarAsistente("u1", "Nombre", "a@b.com");
 ev.ConteoAsistentesActual.Should().Be(1);
 ev.EventosDominio.Should().Contain(e => e is AsistenteRegistradoEventoDominio);
 }

 [Fact]
 public void RegistrarAsistente_Completo_LanzaExcepcion()
 {
 var ev = Build(1);
 ev.Publicar();
 ev.RegistrarAsistente("u1", "Nombre", "a@b.com");
 Action act = () => ev.RegistrarAsistente("u2", "Nombre2", "c@d.com");
 act.Should().Throw<InvalidOperationException>().WithMessage("*completo*");
 }

 [Fact]
 public void RegistrarAsistente_Duplicado_LanzaExcepcion()
 {
 var ev = Build();
 ev.Publicar();
 ev.RegistrarAsistente("u1", "Nombre", "a@b.com");
 Action act = () => ev.RegistrarAsistente("u1", "Nombre", "a@b.com");
 act.Should().Throw<InvalidOperationException>().WithMessage("*ya est*");
 }

 [Fact]
 public void Actualizar_ReduciendoMaximoPorDebajoDelConteo_LanzaExcepcion()
 {
 var ev = Build(3);
 ev.Publicar();
 ev.RegistrarAsistente("u1", "Nombre", "a@b.com");
 ev.RegistrarAsistente("u2", "Nombre2", "b@c.com");
 Action act = () => ev.Actualizar("Titulo2", "Desc", _ubicBase, ev.FechaInicio, ev.FechaFin,1);
 act.Should().Throw<ArgumentException>().WithMessage("*reducir*");
 }

 [Fact]
 public void LimpiarEventosDominio_DespuesDePublicar_DejaListaVacia()
 {
 var ev = Build();
 ev.Publicar();
 ev.EventosDominio.Should().NotBeEmpty();
 ev.LimpiarEventosDominio();
 ev.EventosDominio.Should().BeEmpty();
 }

 [Fact]
 public void RegistrarAsistente_EventoCancelado_LanzaExcepcion()
 {
 var ev = Build();
 ev.Publicar();
 ev.Cancelar();
 Action act = () => ev.RegistrarAsistente("u1", "Nombre", "correo@demo.com");
 act.Should().Throw<InvalidOperationException>().WithMessage("*cancelado*");
 }

 [Fact]
 public void AnularRegistroAsistente_Exitoso_CubreLinea122()
 {
 var ev = Build();
 ev.Publicar();
 ev.RegistrarAsistente("u1","Nombre","a@b.com");
 ev.ConteoAsistentesActual.Should().Be(1);
 ev.AnularRegistroAsistente("u1");
 ev.ConteoAsistentesActual.Should().Be(0);
 }
}
