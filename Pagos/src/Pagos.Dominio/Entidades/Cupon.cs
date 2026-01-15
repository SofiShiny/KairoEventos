namespace Pagos.Dominio.Entidades;

/// <summary>
/// Entidad que representa un cupón de descuento
/// </summary>
public class Cupon
{
    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    
    /// <summary>
    /// Porcentaje de descuento (0-100)
    /// </summary>
    public decimal PorcentajeDescuento { get; private set; }
    
    public TipoCupon Tipo { get; private set; }
    public EstadoCupon Estado { get; private set; }
    
    /// <summary>
    /// ID del evento al que aplica. Null si es global.
    /// </summary>
    public Guid? EventoId { get; private set; }
    
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaExpiracion { get; private set; }
    
    /// <summary>
    /// Para cupones únicos, guarda el ID del usuario que lo usó
    /// </summary>
    public Guid? UsuarioId { get; private set; }
    
    /// <summary>
    /// Fecha en que fue usado (para cupones únicos)
    /// </summary>
    public DateTime? FechaUso { get; private set; }
    
    /// <summary>
    /// Para cupones generales, cuenta cuántas veces se ha usado
    /// </summary>
    public int ContadorUsos { get; private set; }
    
    /// <summary>
    /// Límite de usos para cupones generales. Null = ilimitado
    /// </summary>
    public int? LimiteUsos { get; private set; }

    // Constructor privado para EF Core
    private Cupon() { }

    // Constructor para cupón general
    public static Cupon CrearCuponGeneral(
        string codigo,
        decimal porcentajeDescuento,
        Guid? eventoId = null,
        DateTime? fechaExpiracion = null,
        int? limiteUsos = null)
    {
        ValidarCodigo(codigo);
        ValidarPorcentaje(porcentajeDescuento);

        return new Cupon
        {
            Id = Guid.NewGuid(),
            Codigo = codigo.ToUpper(),
            PorcentajeDescuento = porcentajeDescuento,
            Tipo = TipoCupon.General,
            Estado = EstadoCupon.Activo,
            EventoId = eventoId,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = fechaExpiracion.HasValue ? DateTime.SpecifyKind(fechaExpiracion.Value, DateTimeKind.Utc) : null,
            ContadorUsos = 0,
            LimiteUsos = limiteUsos
        };
    }

    // Constructor para cupón único
    public static Cupon CrearCuponUnico(
        string codigo,
        decimal porcentajeDescuento,
        Guid? eventoId,
        DateTime? fechaExpiracion = null)
    {
        ValidarCodigo(codigo);
        ValidarPorcentaje(porcentajeDescuento);

        return new Cupon
        {
            Id = Guid.NewGuid(),
            Codigo = codigo.ToUpper(),
            PorcentajeDescuento = porcentajeDescuento,
            Tipo = TipoCupon.Unico,
            Estado = EstadoCupon.Activo,
            EventoId = eventoId,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = fechaExpiracion.HasValue ? DateTime.SpecifyKind(fechaExpiracion.Value, DateTimeKind.Utc) : null,
            ContadorUsos = 0
        };
    }

    public bool EsValido(Guid? eventoId = null)
    {
        // Verificar estado
        if (Estado != EstadoCupon.Activo)
            return false;

        // Verificar expiración
        if (FechaExpiracion.HasValue && DateTime.UtcNow > FechaExpiracion.Value)
        {
            Estado = EstadoCupon.Expirado;
            return false;
        }

        // Verificar evento (si el cupón es específico de un evento)
        if (EventoId.HasValue && eventoId.HasValue && EventoId.Value != eventoId.Value)
            return false;

        // Verificar límite de usos para cupones generales
        if (Tipo == TipoCupon.General && LimiteUsos.HasValue && ContadorUsos >= LimiteUsos.Value)
        {
            Estado = EstadoCupon.Agotado;
            return false;
        }

        // Verificar si cupón único ya fue usado
        if (Tipo == TipoCupon.Unico && UsuarioId.HasValue)
        {
            Estado = EstadoCupon.Usado;
            return false;
        }

        return true;
    }

    public void MarcarComoUsado(Guid usuarioId)
    {
        if (Tipo == TipoCupon.Unico)
        {
            UsuarioId = usuarioId;
            FechaUso = DateTime.UtcNow;
            Estado = EstadoCupon.Usado;
        }
        else
        {
            ContadorUsos++;
            if (LimiteUsos.HasValue && ContadorUsos >= LimiteUsos.Value)
            {
                Estado = EstadoCupon.Agotado;
            }
        }
    }

    public decimal CalcularDescuento(decimal montoTotal)
    {
        if (montoTotal <= 0)
            return 0;

        var descuento = montoTotal * (PorcentajeDescuento / 100m);
        
        // El descuento no puede ser mayor que el total
        return Math.Min(descuento, montoTotal);
    }

    private static void ValidarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del cupón no puede estar vacío");

        if (codigo.Length < 3 || codigo.Length > 20)
            throw new ArgumentException("El código debe tener entre 3 y 20 caracteres");
    }

    private static void ValidarPorcentaje(decimal porcentaje)
    {
        if (porcentaje <= 0 || porcentaje > 100)
            throw new ArgumentException("El porcentaje debe estar entre 1 y 100");
    }
}

public enum TipoCupon
{
    General = 1,  // Puede ser usado múltiples veces
    Unico = 2     // Solo puede ser usado una vez
}

public enum EstadoCupon
{
    Activo = 1,
    Usado = 2,
    Expirado = 3,
    Agotado = 4   // Para cupones con límite de usos
}
