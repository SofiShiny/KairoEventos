using System;
using System.Linq;
using Streaming.Dominio.Modelos;

namespace Streaming.Dominio.Entidades;

public class Transmision
{
    public Guid Id { get; private set; }
    public Guid EventoId { get; private set; }
    public PlataformaTransmision Plataforma { get; private set; }
    public string UrlAcceso { get; private set; } = string.Empty;
    public EstadoTransmision Estado { get; private set; }

    // Ef core constructor
    private Transmision() { }

    private Transmision(Guid eventoId, PlataformaTransmision plataforma, string? urlAcceso = null)
    {
        Id = Guid.NewGuid();
        EventoId = eventoId;
        Plataforma = plataforma;
        Estado = EstadoTransmision.Programada;
        UrlAcceso = urlAcceso ?? GenerarUrlMeet();
    }

    public static Transmision Crear(Guid eventoId, PlataformaTransmision plataforma, string? urlAcceso = null)
    {
        return new Transmision(eventoId, plataforma, urlAcceso);
    }

    private static string GenerarUrlMeet()
    {
        // Genera un código de 10 caracteres tipo abc-defg-hij
        var randon = new Random();
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        string GenerarSegmento(int log) => new string(Enumerable.Repeat(chars, log)
            .Select(s => s[randon.Next(s.Length)]).ToArray());

        return $"https://meet.google.com/{GenerarSegmento(3)}-{GenerarSegmento(4)}-{GenerarSegmento(3)}";
    }

    public void IniciarTransmision()
    {
        Estado = EstadoTransmision.EnVivo;
    }

    public void FinalizarTransmision()
    {
        Estado = EstadoTransmision.Finalizada;
    }
}
