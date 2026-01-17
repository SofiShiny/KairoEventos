using BloquesConstruccion.Dominio;
using Asientos.Dominio.ObjetosDeValor;
using System;

namespace Asientos.Dominio.Entidades;

public class Asiento : Entidad
{
    public Guid MapaId { get; private set; }
    public Guid EventoId { get; private set; }
    public int Fila { get; private set; }
    public int Numero { get; private set; }
    public CategoriaAsiento Categoria { get; private set; }
    public bool Reservado { get; private set; }
    public bool Pagado { get; private set; }
    public Guid? UsuarioId { get; private set; }

    // Constructor sin parámetros para EF Core
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private Asiento() 
    { 
        Categoria = null!; 
    }

    internal Asiento(Guid mapaId, Guid eventoId, int fila, int numero, CategoriaAsiento categoria)
    {
        if (mapaId == Guid.Empty) throw new ArgumentException("MapaId requerido", nameof(mapaId));
        if (eventoId == Guid.Empty) throw new ArgumentException("EventoId requerido", nameof(eventoId));
        if (fila < 0) throw new ArgumentException("Fila >= 0", nameof(fila));
        if (numero < 0) throw new ArgumentException("Numero >= 0", nameof(numero));
        
        MapaId = mapaId;
        EventoId = eventoId;
        Fila = fila;
        Numero = numero;
        Categoria = categoria ?? throw new ArgumentNullException(nameof(categoria));
        Reservado = false;
        Pagado = false;
    }

    // Constructor público para pruebas
    public Asiento(Guid mapaId, Guid eventoId, int fila, int numero, CategoriaAsiento categoria, bool forTests)
        : this(mapaId, eventoId, fila, numero, categoria) { }

    public void Reservar(Guid? usuarioId = null)
    {
        if (Reservado) throw new InvalidOperationException("Asiento ya reservado");
        Reservado = true;
        UsuarioId = usuarioId;
        Pagado = false; // Al reservar, aún no está pagado
    }

    public void MarcarComoPagado()
    {
        if (!Reservado) throw new InvalidOperationException("No se puede pagar un asiento no reservado");
        Pagado = true;
    }

    public void Liberar()
    {
        if (!Reservado) return; // Idempotencia
        Reservado = false;
        Pagado = false;
        UsuarioId = null;
    }
}
