using Marketing.Aplicacion.Interfaces;
using Marketing.Dominio.Entidades;

namespace Marketing.Aplicacion.CasosUso;

public class CrearCuponUseCase
{
    private readonly IRepositorioCupones _repositorio;

    public CrearCuponUseCase(IRepositorioCupones repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Guid> EjecutarAsync(string codigo, Dominio.Enums.TipoDescuento tipo, decimal valor, DateTime expiracion)
    {
        var existe = await _repositorio.ObtenerPorCodigoAsync(codigo);
        if (existe != null)
            throw new InvalidOperationException($"El código {codigo} ya está registrado.");

        var cupon = new Cupon(codigo, tipo, valor, expiracion);
        await _repositorio.AgregarAsync(cupon);
        return cupon.Id;
    }
}
