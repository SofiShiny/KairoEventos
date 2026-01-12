using Pagos.Dominio.Entidades;
using Pagos.Dominio.Interfaces;

namespace Pagos.Aplicacion.Servicios;

public interface ICuponServicio
{
    Task<ResultadoValidacionCupon> ValidarCuponAsync(string codigo, Guid? eventoId, decimal montoTotal, CancellationToken cancellationToken = default);
    Task<Cupon> CrearCuponGeneralAsync(CrearCuponGeneralDto dto, CancellationToken cancellationToken = default);
    Task<List<Cupon>> GenerarLoteCuponesAsync(GenerarLoteCuponesDto dto, CancellationToken cancellationToken = default);
    Task<List<CuponDto>> ObtenerCuponesPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);
    Task<List<CuponDto>> ObtenerCuponesGlobalesAsync(CancellationToken cancellationToken = default);
    Task MarcarCuponComoUsadoAsync(string codigo, Guid usuarioId, CancellationToken cancellationToken = default);
}

public class CuponServicio : ICuponServicio
{
    private readonly ICuponRepositorio _cuponRepositorio;

    public CuponServicio(ICuponRepositorio cuponRepositorio)
    {
        _cuponRepositorio = cuponRepositorio;
    }

    public async Task<ResultadoValidacionCupon> ValidarCuponAsync(
        string codigo,
        Guid? eventoId,
        decimal montoTotal,
        CancellationToken cancellationToken = default)
    {
        var cupon = await _cuponRepositorio.ObtenerPorCodigoAsync(codigo, cancellationToken);

        if (cupon == null)
        {
            return new ResultadoValidacionCupon
            {
                EsValido = false,
                Mensaje = "Cupón no encontrado"
            };
        }

        if (!cupon.EsValido(eventoId))
        {
            var mensaje = cupon.Estado switch
            {
                EstadoCupon.Expirado => "El cupón ha expirado",
                EstadoCupon.Usado => "Este cupón ya ha sido utilizado",
                EstadoCupon.Agotado => "El cupón ha alcanzado su límite de usos",
                _ => "El cupón no es válido para este evento"
            };

            return new ResultadoValidacionCupon
            {
                EsValido = false,
                Mensaje = mensaje
            };
        }

        var descuento = cupon.CalcularDescuento(montoTotal);
        var nuevoTotal = Math.Max(0, montoTotal - descuento);

        return new ResultadoValidacionCupon
        {
            EsValido = true,
            Descuento = descuento,
            NuevoTotal = nuevoTotal,
            PorcentajeDescuento = cupon.PorcentajeDescuento,
            Mensaje = $"Cupón aplicado: {cupon.PorcentajeDescuento}% de descuento"
        };
    }

    public async Task<Cupon> CrearCuponGeneralAsync(CrearCuponGeneralDto dto, CancellationToken cancellationToken = default)
    {
        // Verificar que el código no exista
        if (await _cuponRepositorio.ExisteCodigoAsync(dto.Codigo, cancellationToken))
        {
            throw new InvalidOperationException($"Ya existe un cupón con el código '{dto.Codigo}'");
        }

        var cupon = Cupon.CrearCuponGeneral(
            dto.Codigo,
            dto.PorcentajeDescuento,
            dto.EsGlobal ? null : dto.EventoId,
            dto.FechaExpiracion,
            dto.LimiteUsos
        );

        await _cuponRepositorio.AgregarAsync(cupon, cancellationToken);
        return cupon;
    }

    public async Task<List<Cupon>> GenerarLoteCuponesAsync(GenerarLoteCuponesDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Cantidad < 1 || dto.Cantidad > 1000)
        {
            throw new ArgumentException("La cantidad debe estar entre 1 y 1000");
        }

        var cupones = new List<Cupon>();

        for (int i = 0; i < dto.Cantidad; i++)
        {
            string codigo;
            int intentos = 0;
            const int maxIntentos = 10;

            // Generar código único
            do
            {
                codigo = GenerarCodigoAleatorio();
                intentos++;

                if (intentos > maxIntentos)
                {
                    throw new InvalidOperationException("No se pudo generar un código único después de varios intentos");
                }
            }
            while (await _cuponRepositorio.ExisteCodigoAsync(codigo, cancellationToken) ||
                   cupones.Any(c => c.Codigo == codigo));

            var cupon = Cupon.CrearCuponUnico(
                codigo,
                dto.PorcentajeDescuento,
                dto.EventoId,
                dto.FechaExpiracion
            );

            cupones.Add(cupon);
        }

        await _cuponRepositorio.AgregarVariosAsync(cupones, cancellationToken);
        return cupones;
    }

    public async Task<List<CuponDto>> ObtenerCuponesPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default)
    {
        var cupones = await _cuponRepositorio.ObtenerPorEventoAsync(eventoId, cancellationToken);
        return cupones.Select(MapearACuponDto).ToList();
    }

    public async Task<List<CuponDto>> ObtenerCuponesGlobalesAsync(CancellationToken cancellationToken = default)
    {
        var cupones = await _cuponRepositorio.ObtenerGlobalesAsync(cancellationToken);
        return cupones.Select(MapearACuponDto).ToList();
    }

    public async Task MarcarCuponComoUsadoAsync(string codigo, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var cupon = await _cuponRepositorio.ObtenerPorCodigoAsync(codigo, cancellationToken);
        
        if (cupon == null)
        {
            throw new InvalidOperationException("Cupón no encontrado");
        }

        cupon.MarcarComoUsado(usuarioId);
        await _cuponRepositorio.ActualizarAsync(cupon, cancellationToken);
    }

    private static string GenerarCodigoAleatorio()
    {
        const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Sin caracteres confusos (I, O, 0, 1)
        var random = new Random();
        var codigo = new char[8];

        for (int i = 0; i < 8; i++)
        {
            codigo[i] = caracteres[random.Next(caracteres.Length)];
        }

        return new string(codigo);
    }

    private static CuponDto MapearACuponDto(Cupon cupon)
    {
        return new CuponDto
        {
            Id = cupon.Id,
            Codigo = cupon.Codigo,
            PorcentajeDescuento = cupon.PorcentajeDescuento,
            Tipo = cupon.Tipo.ToString(),
            Estado = cupon.Estado.ToString(),
            EventoId = cupon.EventoId,
            FechaCreacion = cupon.FechaCreacion,
            FechaExpiracion = cupon.FechaExpiracion,
            UsosRestantes = cupon.Tipo == TipoCupon.Unico
                ? (cupon.UsuarioId.HasValue ? 0 : 1)
                : (cupon.LimiteUsos.HasValue ? Math.Max(0, cupon.LimiteUsos.Value - cupon.ContadorUsos) : null)
        };
    }
}

// DTOs
public record CrearCuponGeneralDto(
    string Codigo,
    decimal PorcentajeDescuento,
    Guid? EventoId,
    bool EsGlobal,
    DateTime? FechaExpiracion,
    int? LimiteUsos
);

public record GenerarLoteCuponesDto(
    int Cantidad,
    decimal PorcentajeDescuento,
    Guid? EventoId,
    DateTime? FechaExpiracion
);

public record ResultadoValidacionCupon
{
    public bool EsValido { get; init; }
    public decimal Descuento { get; init; }
    public decimal NuevoTotal { get; init; }
    public decimal PorcentajeDescuento { get; init; }
    public string Mensaje { get; init; } = string.Empty;
}

public record CuponDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public decimal PorcentajeDescuento { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public Guid? EventoId { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaExpiracion { get; init; }
    public int? UsosRestantes { get; init; }
}
