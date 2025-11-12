using Eventos.Aplicacion.DTOs;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Eventos.Pruebas.Aplicacion.DTOs;

public class EventoDtoPruebas
{
    [Fact]
    public void EventoDto_DebeInicializarTodasLasPropiedades()
    {
        // Preparar
        var id = Guid.NewGuid();
        var titulo = "Concierto musica";
        var descripcion = "Concierto de musica navideña";
        var fechaInicio = DateTime.UtcNow.AddDays(30);
        var fechaFin = fechaInicio.AddHours(8);
        var maximoAsistentes =100;
        var estado = "Publicado";

        // Ejecutar
        var dto = new EventoDto
        {
            Id = id,
            Titulo = titulo,
            Descripcion = descripcion,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            MaximoAsistentes = maximoAsistentes,
            Estado = estado
        };

        // Comprobar
        dto.Id.Should().Be(id);
        dto.Titulo.Should().Be(titulo);
        dto.Descripcion.Should().Be(descripcion);
        dto.FechaInicio.Should().Be(fechaInicio);
        dto.FechaFin.Should().Be(fechaFin);
        dto.MaximoAsistentes.Should().Be(maximoAsistentes);
        dto.Estado.Should().Be(estado);
    }

    [Fact]
    public void EventoDto_Ubicacion_DebeSerAsignable()
    {
        // Preparar
        var dto = new EventoDto();
        var ubicacionDto = new UbicacionDto
        {
            NombreLugar = "CCCT",
            Direccion = "Av la Estancia, Chuao",
            Ciudad = "Caracas",
            Region = "DF",
            CodigoPostal = "1090",
            Pais = "Venezuela"
        };

        // Ejecutar
        dto.Ubicacion = ubicacionDto;

        // Comprobar
        dto.Ubicacion.Should().NotBeNull();
        dto.Ubicacion!.NombreLugar.Should().Be("CCCT");
        dto.Ubicacion.Ciudad.Should().Be("Caracas");
    }

    [Fact]
    public void EventoDto_Asistentes_DebeSerAsignableYEvaluable()
    {
        // Preparar
        var dto = new EventoDto();
        var asistentes = new List<AsistenteDto>
        {
            new AsistenteDto { Id = Guid.NewGuid(), Nombre = "Creonte Lara", Correo = "cdlara@est.ucab.edu.ve" },
            new AsistenteDto { Id = Guid.NewGuid(), Nombre = "Electra Wilson", Correo = "eywilson@est.ucab.edu.ve" }
        };

        // Ejecutar
        dto.Asistentes = asistentes;

        // Comprobar
        dto.Asistentes.Should().HaveCount(2);
        dto.Asistentes.Should().Contain(a => a.Nombre == "Creonte Lara");
        dto.Asistentes.Should().Contain(a => a.Nombre == "Electra Wilson");
    }

    [Fact]
    public void EventoDto_DebePermitirValoresNulos()
    {
        // Preparar y ejecutar
        var dto = new EventoDto
        {
            Descripcion = null,
            Ubicacion = null,
            Asistentes = null
        };

        // Comprobar
        dto.Descripcion.Should().BeNull();
        dto.Ubicacion.Should().BeNull();
        dto.Asistentes.Should().BeNull();
    }
}
