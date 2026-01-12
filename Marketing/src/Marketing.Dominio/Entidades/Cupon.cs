using Marketing.Dominio.Enums;

namespace Marketing.Dominio.Entidades;

public class Cupon
{
    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public TipoDescuento TipoDescuento { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime FechaExpiracion { get; private set; }
    public EstadoCupon Estado { get; private set; }
    public Guid? UsuarioDestinatarioId { get; private set; }
    public Guid? UsuarioQueLoUso { get; private set; }
    public DateTime? FechaUso { get; private set; }

    // Constructor para EF Core
    private Cupon() { }

    public Cupon(string codigo, TipoDescuento tipo, decimal valor, DateTime expiracion)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("El código es obligatorio.");
        if (valor <= 0) throw new ArgumentException("El valor debe ser positivo.");
        if (expiracion <= DateTime.UtcNow) throw new ArgumentException("La fecha de expiración debe ser futura.");

        Id = Guid.NewGuid();
        Codigo = codigo.ToUpper().Trim();
        TipoDescuento = tipo;
        Valor = valor;
        FechaExpiracion = expiracion;
        Estado = EstadoCupon.Disponible;
    }

    public void AsignarADestinatario(Guid usuarioId)
    {
        if (Estado != EstadoCupon.Disponible)
            throw new InvalidOperationException("Solo se pueden enviar cupones disponibles.");
            
        if (FechaExpiracion <= DateTime.UtcNow)
            throw new InvalidOperationException("El cupón ha expirado.");

        UsuarioDestinatarioId = usuarioId;
    }

    public void MarcarComoUsado(Guid usuarioId)
    {
        if (Estado == EstadoCupon.Usado)
            throw new InvalidOperationException("El cupón ya fue utilizado.");

        if (FechaExpiracion <= DateTime.UtcNow)
            throw new InvalidOperationException("El cupón ha expirado.");

        Estado = EstadoCupon.Usado;
        UsuarioQueLoUso = usuarioId;
        FechaUso = DateTime.UtcNow;
    }

    public bool EsValido() => Estado == EstadoCupon.Disponible && FechaExpiracion > DateTime.UtcNow;

    // Solo para fines de testing o escenarios muy específicos
    internal void ForceExpiracion(DateTime fecha)
    {
        FechaExpiracion = fecha;
    }
}
