using Marketing.Aplicacion.Interfaces;
using Marketing.Dominio.Enums;

namespace Marketing.Aplicacion.CasosUso;

public record ResultadoValidacion(bool EsValido, decimal Valor = 0, TipoDescuento? Tipo = null, string Mensaje = "");

public class ValidarCuponUseCase
{
    private readonly IRepositorioCupones _repositorio;

    public ValidarCuponUseCase(IRepositorioCupones repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ResultadoValidacion> EjecutarAsync(string codigo)
    {
        var cupon = await _repositorio.ObtenerPorCodigoAsync(codigo);

        if (cupon == null)
            return new ResultadoValidacion(false, Mensaje: "Código de cupón inexistente.");

        if (!cupon.EsValido())
        {
            if (cupon.Estado == EstadoCupon.Usado)
                return new ResultadoValidacion(false, Mensaje: "El cupón ya ha sido utilizado.");
            
            if (cupon.FechaExpiracion <= DateTime.UtcNow)
                return new ResultadoValidacion(false, Mensaje: "El cupón ha expirado.");

            return new ResultadoValidacion(false, Mensaje: "Cupón no disponible.");
        }

        return new ResultadoValidacion(true, cupon.Valor, cupon.TipoDescuento, "Cupón aplicado con éxito.");
    }
}
