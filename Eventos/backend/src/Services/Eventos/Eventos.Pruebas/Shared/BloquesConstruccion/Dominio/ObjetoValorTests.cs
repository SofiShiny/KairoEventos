using BloquesConstruccion.Dominio;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Shared.BloquesConstruccion.Dominio;

public class ObjetoValorTests
{
 private readonly Punto _puntoBase;
 private readonly Punto _puntoIgual;
 private readonly Punto _puntoDistinto;
 private readonly PuntoNullable _puntoNullableA;
 private readonly PuntoNullable _puntoNullableB;

 private sealed class Punto : ObjetoValor
 {
 public int X { get; }
 public int Y { get; }
 public Punto(int x,int y){ X=x; Y=y; }
 protected override IEnumerable<object> ObtenerComponentesDeIgualdad() => new object[]{ X, Y };
 }

 private sealed class PuntoNullable : ObjetoValor
 {
 public string? Etiqueta { get; }
 public int Valor { get; }
 public PuntoNullable(string? etiqueta,int valor){ Etiqueta=etiqueta; Valor=valor; }
 protected override IEnumerable<object> ObtenerComponentesDeIgualdad() => new object?[]{ Etiqueta, Valor }!;
 }

 public ObjetoValorTests()
 {
 _puntoBase = new Punto(1,2);
 _puntoIgual = new Punto(1,2);
 _puntoDistinto = new Punto(1,3);
 _puntoNullableA = new PuntoNullable(null,5);
 _puntoNullableB = new PuntoNullable(null,5);
 }

 [Fact]
 public void Equals_MismoTipoYMismosValores_True()
 {
 _puntoBase.Equals(_puntoIgual).Should().BeTrue();
 _puntoBase.GetHashCode().Should().Be(_puntoIgual.GetHashCode());
 }

 [Fact]
 public void Equals_TipoDiferente_False()
 {
 _puntoBase.Equals(new object()).Should().BeFalse();
 }

 [Fact]
 public void Equals_Null_False()
 {
 _puntoBase.Equals(null).Should().BeFalse();
 }

 [Fact]
 public void Equals_MismoTipoValoresDistintos_False()
 {
 _puntoBase.Equals(_puntoDistinto).Should().BeFalse();
 }

 [Fact]
 public void OperadorIgual_ReferenceEquals_True()
 {
 ReferenceEquals(_puntoBase,_puntoBase).Should().BeTrue();
 var b = _puntoBase;
 (_puntoBase == b).Should().BeTrue();
 }

 [Fact]
 public void OperadorIgual_NullComparisons()
 {
 Punto? a = null; Punto? b = null;
 (a == b).Should().BeTrue();
 (_puntoBase == a).Should().BeFalse();
 (_puntoBase == b).Should().BeFalse();
 }

 [Fact]
 public void OperadorNoIgual_Complemento()
 {
 (_puntoBase != _puntoDistinto).Should().BeTrue();
 }

 [Fact]
 public void GetHashCode_ComponentesConNull_NoFalla()
 {
 _puntoNullableA.GetHashCode().Should().Be(_puntoNullableB.GetHashCode());
 }
}
