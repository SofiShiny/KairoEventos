namespace Usuarios.Dominio.Eventos;

public record UsuarioAccionRealizada(
    Guid UsuarioId,
    string Accion, // GET, POST, PUT, DELETE
    string Path,
    string Datos,
    DateTime Fecha
);
