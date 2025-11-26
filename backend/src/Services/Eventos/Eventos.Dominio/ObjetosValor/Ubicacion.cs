using BloquesConstruccion.Dominio;
using System;
using System.Collections.Generic;

namespace Eventos.Dominio.ObjetosDeValor;

public class Ubicacion : ObjetoValor
{
    public string NombreLugar { get; private set; } = string.Empty;
    public string Direccion { get; private set; } = string.Empty;
    public string Ciudad { get; private set; } = string.Empty;
    public string Region { get; private set; } = string.Empty;
    public string CodigoPostal { get; private set; } = string.Empty;
    public string Pais { get; private set; } = string.Empty;

    private Ubicacion() { } // Para EF Core

    public Ubicacion(string nombreLugar, string direccion, string ciudad, string region, string codigoPostal, string pais)
    {
        if (string.IsNullOrWhiteSpace(nombreLugar))
            throw new ArgumentException("El nombre del lugar no puede estar vacio", nameof(nombreLugar));
        
        if (string.IsNullOrWhiteSpace(direccion))
            throw new ArgumentException("La direccion no puede estar vacia", nameof(direccion));
        
        if (string.IsNullOrWhiteSpace(ciudad))
            throw new ArgumentException("La ciudad no puede estar vacia", nameof(ciudad));
        
        if (string.IsNullOrWhiteSpace(pais))
            throw new ArgumentException("El pais no puede estar vacio", nameof(pais));

        NombreLugar = nombreLugar;
        Direccion = direccion;
        Ciudad = ciudad;
        Region = region ?? string.Empty;
        CodigoPostal = codigoPostal ?? string.Empty;
        Pais = pais;
    }

    protected override IEnumerable<object> ObtenerComponentesDeIgualdad()
    {
        yield return NombreLugar;
        yield return Direccion;
        yield return Ciudad;
        yield return Region;
        yield return CodigoPostal;
        yield return Pais;
    }

    public override string ToString() => $"{NombreLugar}, {Direccion}, {Ciudad}, {Region} {CodigoPostal}, {Pais}";
}
