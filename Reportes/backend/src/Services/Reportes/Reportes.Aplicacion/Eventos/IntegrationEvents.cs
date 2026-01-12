using System;

namespace Pagos.Aplicacion.Eventos
{
    public record PagoAprobadoEvento(
        Guid TransaccionId,
        Guid OrdenId,
        Guid UsuarioId,
        decimal Monto,
        string UrlFactura
    );
}

namespace Reportes.Aplicacion.Eventos
{
    public record EntradaCompradaEvento(
        Guid EntradaId,
        Guid UsuarioId,
        Guid EventoId,
        string Categoria
    );

    public record ServicioReservadoEvento(
        Guid ReservaId,
        Guid UsuarioId,
        Guid EventoId,
        string NombreServicio,
        decimal Precio
    );
}
