using BloquesConstruccion.Dominio;
using Asientos.Dominio.Entidades;
using Asientos.Dominio.ObjetosDeValor;
using Asientos.Dominio.EventosDominio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Asientos.Dominio.Agregados;

public class MapaAsientos : RaizAgregada
{
    private readonly List<Asiento> _asientos = new();
    private readonly List<CategoriaAsiento> _categorias = new();
    public Guid EventoId { get; private set; }
    public IReadOnlyCollection<Asiento> Asientos => _asientos.AsReadOnly();
    public IReadOnlyCollection<CategoriaAsiento> Categorias => _categorias.AsReadOnly();

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private MapaAsientos() { }

    private MapaAsientos(Guid eventoId)
    {
        if (eventoId == Guid.Empty) throw new ArgumentException("EventoId requerido", nameof(eventoId));
        Id = Guid.NewGuid();
        EventoId = eventoId;
        GenerarEventoDominio(new MapaAsientosCreadoEventoDominio(Id, eventoId));
    }

    public static MapaAsientos Crear(Guid eventoId)
    {
        return new MapaAsientos(eventoId);
    }

    public Asiento AgregarAsiento(int fila, int numero, string categoriaNombre)
    {
        var cat = _categorias.FirstOrDefault(c => c.Nombre.Equals(categoriaNombre, StringComparison.OrdinalIgnoreCase))
                  ?? throw new InvalidOperationException("Categoria no definida");

        if (_asientos.Any(a => a.Fila == fila && a.Numero == numero))
            throw new InvalidOperationException("Asiento duplicado");

        var asiento = new Asiento(Id, EventoId, fila, numero, CategoriaAsiento.Crear(cat.Nombre, cat.PrecioBase, cat.TienePrioridad));
        _asientos.Add(asiento);
        GenerarEventoDominio(new AsientoAgregadoEventoDominio(Id, fila, numero, cat.Nombre));
        return asiento;
    }

    public void ReservarAsiento(int fila, int numero, Guid? usuarioId = null)
    {
        var asiento = _asientos.FirstOrDefault(a => a.Fila == fila && a.Numero == numero)
                      ?? throw new InvalidOperationException("Asiento inexistente");
        asiento.Reservar(usuarioId);
        GenerarEventoDominio(new AsientoReservadoEventoDominio(Id, fila, numero));
    }

    public void ReservarAsientoPorId(Guid asientoId, Guid usuarioId)
    {
        var asiento = _asientos.FirstOrDefault(a => a.Id == asientoId)
                      ?? throw new InvalidOperationException("Asiento inexistente");
        
        if (asiento.Reservado && asiento.UsuarioId != usuarioId)
            throw new InvalidOperationException("Asiento ya reservado por otro usuario");

        asiento.Reservar(usuarioId);
        GenerarEventoDominio(new AsientoReservadoEventoDominio(Id, asiento.Fila, asiento.Numero));
    }

    public void LiberarAsiento(int fila, int numero)
    {
        var asiento = _asientos.FirstOrDefault(a => a.Fila == fila && a.Numero == numero)
                      ?? throw new InvalidOperationException("Asiento inexistente");
        asiento.Liberar();
        GenerarEventoDominio(new AsientoLiberadoEventoDominio(Id, asiento.Id, fila, numero));
    }

    public void LiberarAsientoPorId(Guid asientoId)
    {
        var asiento = _asientos.FirstOrDefault(a => a.Id == asientoId)
                      ?? throw new InvalidOperationException("Asiento inexistente");
        asiento.Liberar();
        GenerarEventoDominio(new AsientoLiberadoEventoDominio(Id, asiento.Id, asiento.Fila, asiento.Numero));
    }

    public CategoriaAsiento AgregarCategoria(string nombre, decimal? precio, bool tienePrioridad)
    {
        if (_categorias.Any(c => c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Categoria ya existe");
        
        var cat = CategoriaAsiento.Crear(nombre, precio, tienePrioridad);
        _categorias.Add(cat);
        GenerarEventoDominio(new CategoriaAgregadaEventoDominio(Id, nombre));
        return cat;
    }
}
