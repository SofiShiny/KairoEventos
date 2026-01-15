using Entradas.Dominio.Enums;
using Entradas.Dominio.Excepciones;

namespace Entradas.Dominio.Entidades;

/// <summary>
/// Entidad que representa una entrada (ticket) para un evento
/// </summary>
public class Entrada : EntidadBase
{
    /// <summary>
    /// Identificador del evento al que pertenece la entrada
    /// </summary>
    public Guid EventoId { get; protected set; }
    
    /// <summary>
    /// Identificador del usuario que compró la entrada
    /// </summary>
    public Guid UsuarioId { get; protected set; }
    
    /// <summary>
    /// Identificador del asiento asignado (opcional para entradas generales)
    /// </summary>
    public Guid? AsientoId { get; protected set; }
    
    /// <summary>
    /// Monto original de la entrada antes de descuentos
    /// </summary>
    public decimal MontoOriginal { get; protected set; }

    /// <summary>
    /// Monto descontado de la entrada
    /// </summary>
    public decimal MontoDescuento { get; protected set; }

    /// <summary>
    /// Monto final pagado por la entrada (MontoOriginal - MontoDescuento)
    /// </summary>
    public decimal Monto { get; protected set; }
    
    /// <summary>
    /// Lista de cupones aplicados (almacenado como JSON)
    /// </summary>
    public string? CuponesAplicados { get; protected set; }

    /// <summary>
    /// Código QR único para identificar la entrada
    /// </summary>
    public string CodigoQr { get; protected set; } = string.Empty;
    
    /// <summary>
    /// Estado actual de la entrada
    /// </summary>
    public EstadoEntrada Estado { get; protected set; }
    
    /// <summary>
    /// Fecha y hora de compra de la entrada
    /// </summary>
    public DateTime FechaCompra { get; protected set; }

    // ==================== SNAPSHOT FIELDS (Desnormalización) ====================
    // Estos campos almacenan información descriptiva del evento y asiento
    // para mejorar la experiencia del usuario en el historial sin necesidad
    // de consultas adicionales a otros microservicios.

    /// <summary>
    /// Título del evento (snapshot al momento de la compra)
    /// </summary>
    public string? TituloEvento { get; protected set; }

    /// <summary>
    /// URL de la imagen del evento (snapshot al momento de la compra)
    /// </summary>
    public string? ImagenEventoUrl { get; protected set; }

    /// <summary>
    /// Categoría del evento (snapshot al momento de la compra)
    /// </summary>
    public string? Categoria { get; protected set; }

    /// <summary>
    /// Fecha del evento (snapshot al momento de la compra)
    /// </summary>
    public DateTime? FechaEvento { get; protected set; }

    /// <summary>
    /// Nombre del sector del asiento (snapshot al momento de la compra)
    /// </summary>
    public string? NombreSector { get; protected set; }

    /// <summary>
    /// Fila del asiento (snapshot al momento de la compra)
    /// </summary>
    public string? Fila { get; protected set; }

    /// <summary>
    /// Número del asiento (snapshot al momento de la compra)
    /// </summary>
    public int? NumeroAsiento { get; protected set; }
    
    /// <summary>
    /// Indica si el evento es virtual (snapshot al momento de la compra)
    /// </summary>
    public bool EsVirtual { get; protected set; }

    /// <summary>
    /// Nombre del usuario (snapshot al momento de la compra)
    /// </summary>
    public string? NombreUsuario { get; protected set; }

    /// <summary>
    /// Email del usuario (snapshot al momento de la compra)
    /// </summary>
    public string? EmailUsuario { get; protected set; }

    // Constructor protegido para EF Core
    protected Entrada() : base() { }

    // Constructor privado para el factory method
    private Entrada(
        Guid eventoId, 
        Guid usuarioId, 
        decimal montoOriginal, 
        Guid? asientoId, 
        string codigoQr, 
        decimal montoDescuento = 0, 
        string? cuponesJson = null,
        string? tituloEvento = null,
        string? imagenEventoUrl = null,
        string? categoria = null,
        DateTime? fechaEvento = null,
        string? nombreSector = null,
        string? fila = null,
        int? numeroAsiento = null,
        bool esVirtual = false,
        string? nombreUsuario = null,
        string? emailUsuario = null) 
    {
        // Inicializar ID y fechas para nueva entrada
        Id = Guid.NewGuid();
        FechaCreacion = DateTime.UtcNow;
        FechaActualizacion = DateTime.UtcNow;
        
        EventoId = eventoId;
        UsuarioId = usuarioId;
        AsientoId = asientoId;
        MontoOriginal = montoOriginal;
        MontoDescuento = montoDescuento;
        Monto = Math.Max(0, montoOriginal - montoDescuento);
        CuponesAplicados = cuponesJson;
        CodigoQr = codigoQr ?? throw new ArgumentNullException(nameof(codigoQr));
        Estado = EstadoEntrada.Reservada;
        FechaCompra = DateTime.UtcNow;
        
        // Snapshot fields
        TituloEvento = tituloEvento;
        ImagenEventoUrl = imagenEventoUrl;
        Categoria = categoria;
        FechaEvento = fechaEvento;
        NombreSector = nombreSector;
        Fila = fila;
        NumeroAsiento = numeroAsiento;
        EsVirtual = esVirtual;
        NombreUsuario = nombreUsuario;
        EmailUsuario = emailUsuario;
    }

    /// <summary>
    /// Factory method para crear una nueva entrada con snapshot de datos descriptivos
    /// </summary>
    /// <param name="eventoId">ID del evento</param>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="montoOriginal">Monto base de la entrada</param>
    /// <param name="asientoId">ID del asiento (opcional)</param>
    /// <param name="codigoQr">Código QR único</param>
    /// <param name="montoDescuento">Monto a descontar (opcional)</param>
    /// <param name="cuponesJson">JSON con cupones aplicados (opcional)</param>
    /// <param name="tituloEvento">Título del evento (snapshot)</param>
    /// <param name="imagenEventoUrl">URL de la imagen del evento (snapshot)</param>
    /// <param name="fechaEvento">Fecha del evento (snapshot)</param>
    /// <param name="nombreSector">Nombre del sector (snapshot)</param>
    /// <param name="fila">Fila del asiento (snapshot)</param>
    /// <param name="numeroAsiento">Número del asiento (snapshot)</param>
    /// <returns>Nueva instancia de Entrada</returns>
    public static Entrada Crear(
        Guid eventoId, 
        Guid usuarioId, 
        decimal montoOriginal, 
        Guid? asientoId, 
        string codigoQr, 
        decimal montoDescuento = 0, 
        string? cuponesJson = null,
        string? tituloEvento = null,
        string? imagenEventoUrl = null,
        string? categoria = null,
        DateTime? fechaEvento = null,
        string? nombreSector = null,
        string? fila = null,
        int? numeroAsiento = null,
        bool esVirtual = false,
        string? nombreUsuario = null,
        string? emailUsuario = null)
    {
        ValidarParametrosCreacion(eventoId, usuarioId, montoOriginal, codigoQr);
        
        return new Entrada(
            eventoId, 
            usuarioId, 
            montoOriginal, 
            asientoId, 
            codigoQr, 
            montoDescuento, 
            cuponesJson,
            tituloEvento,
            imagenEventoUrl,
            categoria,
            fechaEvento,
            nombreSector,
            fila,
            numeroAsiento,
            esVirtual,
            nombreUsuario,
            emailUsuario);
    }

    /// <summary>
    /// Confirma el pago de la entrada, cambiando su estado a Pagada
    /// </summary>
    /// <exception cref="EstadoEntradaInvalidoException">Cuando el estado actual no permite la confirmación</exception>
    public void ConfirmarPago()
    {
        if (Estado != EstadoEntrada.PendientePago && Estado != EstadoEntrada.Reservada)
        {
            throw new EstadoEntradaInvalidoException(
                $"No se puede confirmar el pago de una entrada en estado {Estado}. " +
                "Solo se pueden confirmar entradas en estado PendientePago o Reservada.");
        }

        Estado = EstadoEntrada.Pagada;
        ActualizarFechaModificacion();
    }

    /// <summary>
    /// Asigna un nuevo código QR a la entrada
    /// </summary>
    public void AsignarCodigoQr(string nuevoQr)
    {
        if (string.IsNullOrWhiteSpace(nuevoQr))
            throw new ArgumentException("El código QR no puede ser vacío.", nameof(nuevoQr));

        CodigoQr = nuevoQr;
        ActualizarFechaModificacion();
    }

    /// <summary>
    /// Cancela la entrada, cambiando su estado a Cancelada
    /// </summary>
    /// <exception cref="EstadoEntradaInvalidoException">Cuando el estado actual no permite la cancelación</exception>
    public void Cancelar()
    {
        if (Estado == EstadoEntrada.Usada)
        {
            throw new EstadoEntradaInvalidoException(
                "No se puede cancelar una entrada que ya ha sido usada.");
        }

        if (Estado == EstadoEntrada.Cancelada)
        {
            throw new EstadoEntradaInvalidoException(
                "La entrada ya se encuentra cancelada.");
        }

        Estado = EstadoEntrada.Cancelada;
        ActualizarFechaModificacion();
    }

    /// <summary>
    /// Marca la entrada como usada
    /// </summary>
    /// <exception cref="EstadoEntradaInvalidoException">Cuando el estado actual no permite marcar como usada</exception>
    public void MarcarComoUsada()
    {
        if (Estado != EstadoEntrada.Pagada)
        {
            throw new EstadoEntradaInvalidoException(
                $"No se puede usar una entrada en estado {Estado}. " +
                "Solo se pueden usar entradas en estado Pagada.");
        }

        Estado = EstadoEntrada.Usada;
        ActualizarFechaModificacion();
    }

    /// <summary>
    /// Valida los parámetros de creación de una entrada
    /// </summary>
    private static void ValidarParametrosCreacion(Guid eventoId, Guid usuarioId, decimal monto, string codigoQr)
    {
        if (eventoId == Guid.Empty)
        {
            throw new ArgumentException("El ID del evento no puede ser vacío.", nameof(eventoId));
        }

        if (usuarioId == Guid.Empty)
        {
            throw new ArgumentException("El ID del usuario no puede ser vacío.", nameof(usuarioId));
        }

        if (monto < 0)
        {
            throw new ArgumentException("El monto no puede ser negativo.", nameof(monto));
        }

        if (string.IsNullOrWhiteSpace(codigoQr))
        {
            throw new ArgumentException("El código QR no puede ser nulo o vacío.", nameof(codigoQr));
        }
    }
}
