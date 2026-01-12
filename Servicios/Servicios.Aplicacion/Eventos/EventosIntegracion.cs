namespace Servicios.Aplicacion.Eventos;

// Evento que publicamos nosotros
public record SolicitudPagoServicioCreada(
    Guid ReservaId, 
    Guid UsuarioId, 
    decimal Monto, 
    string Descripcion
);

public record PagoAprobadoEvento(
    Guid TransaccionId, 
    Guid OrdenId, // En este caso será el ReservaId
    Guid UsuarioId, 
    decimal Monto, 
    string UrlFactura
);

// Evento de cambio de estado de proveedor
public record ProveedorEstadoCambiadoEvent(
    Guid ServicioId,
    string NombreProveedor,
    bool NuevoEstado
);
