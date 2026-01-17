namespace Servicios.Dominio.Entidades;

public enum EstadoReserva
{
    PendientePago,
    Confirmado,
    Cancelado,
    Rechazado
}

public class ServicioGlobal
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public decimal Precio { get; private set; }
    public bool Activo { get; private set; }
    
    // Nueva relación con proveedores
    private readonly List<ProveedorServicio> _proveedores = new();
    public IReadOnlyCollection<ProveedorServicio> Proveedores => _proveedores.AsReadOnly();

    // Constructor para EF
    private ServicioGlobal() { }

    public ServicioGlobal(Guid id, string nombre, decimal precio)
    {
        Id = id;
        Nombre = nombre;
        Precio = precio;
        Activo = true;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;

    public void ActualizarPrecio(decimal nuevoPrecio) => Precio = nuevoPrecio;

    public void AgregarProveedor(ProveedorServicio proveedor)
    {
        _proveedores.Add(proveedor);
    }
}

public class ProveedorServicio
{
    public Guid Id { get; private set; }
    public Guid ServicioId { get; private set; }
    public string NombreProveedor { get; private set; } = string.Empty;
    public decimal Precio { get; private set; }
    public bool EstaDisponible { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;

    // Constructor para EF
    private ProveedorServicio() { }

    public ProveedorServicio(Guid id, Guid servicioId, string nombreProveedor, decimal precio, string externalId)
    {
        Id = id;
        ServicioId = servicioId;
        NombreProveedor = nombreProveedor;
        Precio = precio;
        ExternalId = externalId;
        EstaDisponible = true;
    }

    public void SetDisponibilidad(bool disponible)
    {
        EstaDisponible = disponible;
    }

    public void ActualizarPrecio(decimal nuevoPrecio)
    {
        Precio = nuevoPrecio;
    }
}

public class ReservaServicio
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid EventoId { get; private set; }
    public Guid ServicioGlobalId { get; private set; }
    public Guid? OrdenEntradaId { get; private set; } // Nuevo campo para correlación
    public EstadoReserva Estado { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    // Constructor para EF
    private ReservaServicio() { }

    public ReservaServicio(Guid usuarioId, Guid eventoId, Guid servicioGlobalId, Guid? ordenEntradaId = null)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        EventoId = eventoId;
        ServicioGlobalId = servicioGlobalId;
        OrdenEntradaId = ordenEntradaId;
        Estado = EstadoReserva.PendientePago;
        FechaCreacion = DateTime.UtcNow;
    }

    public void ConfirmarPago()
    {
        if (Estado != EstadoReserva.PendientePago)
            throw new InvalidOperationException("Solo se puede confirmar el pago de reservas pendientes");
        
        Estado = EstadoReserva.Confirmado;
    }

    public void Cancelar()
    {
        Estado = EstadoReserva.Cancelado;
    }

    public void Rechazar()
    {
        Estado = EstadoReserva.Rechazado;
    }
}
