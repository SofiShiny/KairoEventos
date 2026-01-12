using Pagos.Dominio.Modelos;

namespace Pagos.Dominio.Entidades;

public class Transaccion
{
    public Guid Id { get; set; }
    public Guid OrdenId { get; set; }
    public Guid UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public string TarjetaMascara { get; set; } = string.Empty;
    public EstadoTransaccion Estado { get; set; }
    public string? UrlFactura { get; set; }
    public string? MensajeError { get; set; }
    public DateTime FechaCreacion { get; set; }

    public void Aprobar(string urlFactura)
    {
        Estado = EstadoTransaccion.Aprobado;
        UrlFactura = urlFactura;
        MensajeError = null;
    }

    public void Rechazar(string motivo)
    {
        Estado = EstadoTransaccion.Rechazado;
        MensajeError = motivo;
    }
}
