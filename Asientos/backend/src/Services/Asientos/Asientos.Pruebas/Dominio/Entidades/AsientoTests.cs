using Asientos.Dominio.Agregados;
using Asientos.Dominio.Entidades;
using Asientos.Dominio.ObjetosDeValor;
using FluentAssertions;
using System;
using Xunit;

namespace Asientos.Pruebas.Dominio.Entidades;

public class AsientoTests
{
    private readonly CategoriaAsiento _categoria = CategoriaAsiento.Crear("General", 100m, false);

    [Fact]
    public void Constructor_SiCategoriaEsNula_Falla()
    {
        Action act = () => new Asiento(Guid.NewGuid(), Guid.NewGuid(), 1, 1, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Reservar_SiYaEstaReservado_Falla()
    {
        var asiento = new Asiento(Guid.NewGuid(), Guid.NewGuid(), 1, 1, _categoria, true);
        asiento.Reservar(Guid.NewGuid());

        Action act = () => asiento.Reservar(Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Liberar_Idempotencia()
    {
        var asiento = new Asiento(Guid.NewGuid(), Guid.NewGuid(), 1, 1, _categoria, true);
        asiento.Liberar(); // No debe fallar
        asiento.Reservado.Should().BeFalse();
    }
}
