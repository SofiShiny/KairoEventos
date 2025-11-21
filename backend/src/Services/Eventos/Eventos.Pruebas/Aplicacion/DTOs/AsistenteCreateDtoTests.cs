using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class AsistenteCreateDtoTests
{
 private readonly Guid _id;
 private readonly AsistenteCreateDto _dtoBase;

 public AsistenteCreateDtoTests()
 {
 _id = Guid.NewGuid();
 _dtoBase = new AsistenteCreateDto{ Id = _id, Nombre="N", Correo="a@b.com" };
 }

 [Fact]
 public void SetPropiedades_IdNullable()
 {
 _dtoBase.Id.Should().Be(_id);
 _dtoBase.Nombre.Should().Be("N");
 }

 [Fact]
 public void Defaults_Nulos()
 {
 var dto = new AsistenteCreateDto();
 dto.Id.Should().BeNull();
 dto.Nombre.Should().BeNull();
 dto.Correo.Should().BeNull();
 }
}
