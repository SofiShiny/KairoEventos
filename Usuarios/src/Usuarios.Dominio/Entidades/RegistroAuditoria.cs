namespace Usuarios.Dominio.Entidades;

public class RegistroAuditoria
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Accion { get; private set; } = string.Empty; // POST, PUT, DELETE
    public string Path { get; private set; } = string.Empty;
    public string Datos { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }

    // Constructor para EF
    private RegistroAuditoria() { }

    public RegistroAuditoria(Guid usuarioId, string accion, string path, string datos, DateTime fecha)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Accion = accion;
        Path = path;
        Datos = datos;
        Fecha = fecha;
    }

    public static RegistroAuditoria Crear(Guid usuarioId, string accion, string path, string datos)
    {
        return new RegistroAuditoria(usuarioId, accion, path, datos, DateTime.UtcNow);
    }
}
