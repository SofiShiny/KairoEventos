using BloquesConstruccion.Dominio;
using System;
using System.Net.Mail;

namespace Eventos.Dominio.Entidades;

public class Asistente : Entidad<Guid>
{
    public Guid EventoId { get; private set; }
    public string UsuarioId { get; private set; } = string.Empty;
    public string NombreUsuario { get; private set; } = string.Empty;
    public string Correo { get; private set; } = string.Empty;
    public DateTime RegistradoEn { get; private set; }

    private Asistente() { } // Para EF Core

    public Asistente(Guid eventoId, string usuarioId, string nombreUsuario, string correo)
    {
        if (eventoId == Guid.Empty)
            throw new ArgumentException("El identificador del evento no puede estar vacío", nameof(eventoId));
        
        if (string.IsNullOrWhiteSpace(usuarioId))
            throw new ArgumentException("El identificador del usuario no puede estar vacío", nameof(usuarioId));
        
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(nombreUsuario));
        
        if (string.IsNullOrWhiteSpace(correo))
            throw new ArgumentException("El correo no puede estar vacío", nameof(correo));

        // Validar formato de email
        try
        {
            var _ = new MailAddress(correo);
        }
        catch
        {
            throw new ArgumentException("El email no es válido", nameof(correo));
        }

        Id = Guid.NewGuid();
        EventoId = eventoId;
        UsuarioId = usuarioId;
        NombreUsuario = nombreUsuario;
        Correo = correo;
        RegistradoEn = DateTime.UtcNow;
    }
}
