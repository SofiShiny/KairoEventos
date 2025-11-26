using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class AsistenteDtoPruebas
{
    [Fact]
    public void AsistenteDto_DebeInicializarTodasLasPropiedades()
    {
        // Preparar
        var id = Guid.NewGuid();
        var nombre = "Electra Yocasta Wilson Dominguez";
        var correo = "eywilson@est.ucab.edu.ve";
        var registradoEn = DateTime.UtcNow;

        // Ejecutar
        var dto = new AsistenteDto
        {
            Id = id,
            Nombre = nombre,
            Correo = correo,
            RegistradoEn = registradoEn
        };

        // Comprobar
        dto.Id.Should().Be(id);
        dto.Nombre.Should().Be(nombre);
        dto.Correo.Should().Be(correo);
        dto.RegistradoEn.Should().Be(registradoEn);
    }

    [Fact]
    public void AsistenteDto_DebePermitirValoresNulos()
    {
        // Preparar y ejecutar
        var dto = new AsistenteDto
        {
            Nombre = null,
            Correo = null
        };

        // Comprobar
        dto.Nombre.Should().BeNull();
        dto.Correo.Should().BeNull();
    }
}
