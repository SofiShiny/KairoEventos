using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class RegistrarAsistenteDtoTests
{
 private readonly RegistrarAsistenteDto _dto;

 public RegistrarAsistenteDtoTests()
 {
 _dto = new RegistrarAsistenteDto{ UsuarioId="u1", Nombre="Nombre", Correo="a@b.com" };
 }

 [Fact]
 public void Propiedades_Asignadas()
 {
 _dto.UsuarioId.Should().Be("u1");
 _dto.Nombre.Should().Be("Nombre");
 _dto.Correo.Should().Be("a@b.com");
 }

 [Fact]
 public void ValoresPorDefecto_SonCadenasVacias()
 {
 var d = new RegistrarAsistenteDto();
 d.UsuarioId.Should().BeEmpty();
 d.Nombre.Should().BeEmpty();
 d.Correo.Should().BeEmpty();
 }
}
