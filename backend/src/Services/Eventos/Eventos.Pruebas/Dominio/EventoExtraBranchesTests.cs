using Eventos.Dominio.Entidades;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Dominio.Enumeraciones;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Dominio;

public class EventoBranchesTests
{
 private readonly Ubicacion _ubic;
 private readonly DateTime _inicio;
 private readonly DateTime _fin;

 public EventoBranchesTests()
 {
 _ubic = new Ubicacion("Lugar","Dir","Ciudad","Reg","0000","Pais");
 _inicio = DateTime.UtcNow.AddDays(2);
 _fin = _inicio.AddDays(1);
 }

 private Evento Build() => new("Titulo","Desc", _ubic, _inicio, _fin,5,"org-1");

 [Fact]
 public void Cancelar_YaCancelado_LanzaExcepcion()
 {
 var ev = Build();
 ev.Publicar();
 ev.Cancelar();
 Action act = () => ev.Cancelar();
 act.Should().Throw<InvalidOperationException>().WithMessage("*ya est*cancelad*");
 }

 [Fact]
 public void Cancelar_Completado_LanzaExcepcion()
 {
 var ev = Build();
 ev.Publicar();
 typeof(Evento).GetProperty("Estado")!.SetValue(ev, EstadoEvento.Completado);
 Action act = () => ev.Cancelar();
 act.Should().Throw<InvalidOperationException>().WithMessage("*completado*");
 }

 [Fact]
 public void Actualizar_EventoCancelado_LanzaExcepcion()
 {
 var ev = Build();
 ev.Publicar();
 ev.Cancelar();
 Action act = () => ev.Actualizar("T2","D2", _ubic, ev.FechaInicio, ev.FechaFin,5);
 act.Should().Throw<InvalidOperationException>().WithMessage("*cancelad*");
 }

 [Fact]
 public void Actualizar_EventoCompletado_LanzaExcepcion()
 {
 var ev = Build();
 ev.Publicar();
 typeof(Evento).GetProperty("Estado")!.SetValue(ev, EstadoEvento.Completado);
 Action act = () => ev.Actualizar("T2","D2", _ubic, ev.FechaInicio, ev.FechaFin,5);
 act.Should().Throw<InvalidOperationException>().WithMessage("*completad*");
 }

 [Fact]
 public void Actualizar_TituloVacio_LanzaExcepcion()
 {
 var ev = Build();
 Action act = () => ev.Actualizar(" ","D2", _ubic, ev.FechaInicio, ev.FechaFin,5);
 act.Should().Throw<ArgumentException>().WithMessage("*título*");
 }

 [Fact]
 public void AnularRegistro_NoExiste_LanzaExcepcion()
 {
 var ev = Build();
 ev.Publicar();
 Action act = () => ev.AnularRegistroAsistente("u1");
 act.Should().Throw<InvalidOperationException>().WithMessage("*no est*registrad*");
 }
}
