using System;
using System.Collections.Generic;

namespace BloquesConstruccion.Dominio;

public abstract class RaizAgregada<TId> where TId : struct
{
    public TId Id { get; protected set; }
    public DateTime CreadoEn { get; protected set; } = DateTime.UtcNow;
    public DateTime? ActualizadoEn { get; protected set; }
    
    private readonly List<EventoDominio> _eventosDominio = new();

    public IReadOnlyList<EventoDominio> EventosDominio => _eventosDominio.AsReadOnly();

    protected RaizAgregada()
    {
    }

    protected RaizAgregada(TId id)
    {
        Id = id;
    }

    protected void GenerarEventoDominio(EventoDominio eventoDominio)
    {
        _eventosDominio.Add(eventoDominio);
    }

    public void LimpiarEventosDominio()
    {
        _eventosDominio.Clear();
    }
}

public abstract class RaizAgregada
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreadoEn { get; protected set; } = DateTime.UtcNow;
    public DateTime? ActualizadoEn { get; protected set; }
    
    private readonly List<EventoDominio> _eventosDominio = new();

    public IReadOnlyList<EventoDominio> EventosDominio => _eventosDominio.AsReadOnly();

    protected void GenerarEventoDominio(EventoDominio eventoDominio)
    {
        _eventosDominio.Add(eventoDominio);
    }

    public void LimpiarEventosDominio()
    {
        _eventosDominio.Clear();
    }
}
