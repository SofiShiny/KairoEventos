namespace Reportes.Aplicacion.Eventos;

public record UsuarioAccionRealizada(
    Guid UsuarioId,
    string Accion,
    string Path,
    string Datos,
    DateTime Fecha
);
