using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class AsistenteUpdateDtoTests
{
 private readonly AsistenteUpdateDto _dto;

 public AsistenteUpdateDtoTests()
 {
 _dto = new AsistenteUpdateDto{ Nombre = "Nombre", Correo = "a@b.com" };
 }

 [Fact]
 public void Propiedades_Asignadas()
 {
 _dto.Nombre.Should().Be("Nombre");
 _dto.Correo.Should().Be("a@b.com");
 }

 [Fact]
 public void PermiteNulos()
 {
 var d = new AsistenteUpdateDto();
 d.Nombre.Should().BeNull();
 d.Correo.Should().BeNull();
 }
}
