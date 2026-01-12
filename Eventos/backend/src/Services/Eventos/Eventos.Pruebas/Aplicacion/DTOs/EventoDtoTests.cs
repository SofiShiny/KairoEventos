using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class EventoDtoTests
{
    private readonly EventoDto _dto;
    private readonly Guid _id;
    private readonly DateTime _inicio;
    private readonly DateTime _fin;
    private readonly UbicacionDto _ubic;

    public EventoDtoTests()
    {
        _id = Guid.NewGuid();
        _inicio = DateTime.UtcNow.AddDays(10);
        _fin = _inicio.AddHours(4);
        _ubic = new UbicacionDto{ NombreLugar="Lugar", Direccion="Dir", Ciudad="Ciudad", Pais="Pais" };
        _dto = new EventoDto
        {
            Id = _id,
            Titulo = "Titulo",
            Descripcion = "Desc",
            Ubicacion = _ubic,
            FechaInicio = _inicio,
            FechaFin = _fin,
            MaximoAsistentes =50,
            Estado = "Publicado",
            Asistentes = new List<AsistenteDto>{ new(){ Id = Guid.NewGuid(), NombreUsuario="User", Correo="user@demo.com" } }
        };
    }

    [Fact]
    public void Propiedades_Asignadas()
    {
        _dto.Id.Should().Be(_id);
        _dto.Ubicacion.Should().Be(_ubic);
        _dto.MaximoAsistentes.Should().Be(50);
        _dto.Asistentes.Should().HaveCount(1);
    }

    [Fact]
    public void PermiteNulos()
    {
        var d = new EventoDto();
        d.Descripcion.Should().BeNull();
        d.Ubicacion.Should().BeNull();
        d.Asistentes.Should().BeNull();
    }
}
