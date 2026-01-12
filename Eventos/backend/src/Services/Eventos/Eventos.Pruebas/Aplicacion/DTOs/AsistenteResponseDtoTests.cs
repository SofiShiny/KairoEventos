using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class AsistenteResponseDtoTests
{
 private readonly AsistenteResponseDto _dto;
 private readonly Guid _id;
 private readonly DateTime _registro;

 public AsistenteResponseDtoTests()
 {
 _id = Guid.NewGuid();
 _registro = DateTime.UtcNow;
 _dto = new AsistenteResponseDto{ Id = _id, Nombre="Nombre", Correo="a@b.com", RegistradoEn = _registro };
 }

 [Fact]
 public void Propiedades_Asignadas()
 {
 _dto.Id.Should().Be(_id);
 _dto.Nombre.Should().Be("Nombre");
 _dto.Correo.Should().Be("a@b.com");
 _dto.RegistradoEn.Should().Be(_registro);
 }
}
