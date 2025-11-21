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
            throw new ArgumentException("El nombre del lugar no puede estar vacío", nameof(nombreLugar));
        
        if (string.IsNullOrWhiteSpace(direccion))
            throw new ArgumentException("La dirección no puede estar vacía", nameof(direccion));
        
        if (string.IsNullOrWhiteSpace(ciudad))
            throw new ArgumentException("La ciudad no puede estar vacía", nameof(ciudad));
        
        if (string.IsNullOrWhiteSpace(pais))
            throw new ArgumentException("El país no puede estar vacío", nameof(pais));

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
