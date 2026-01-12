using Marketing.Aplicacion.Interfaces;

namespace Marketing.Aplicacion.CasosUso;

public class ConsumirCuponUseCase
{
    private readonly IRepositorioCupones _repositorio;

    public ConsumirCuponUseCase(IRepositorioCupones repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task EjecutarAsync(string codigo, Guid usuarioId)
    {
        var cupon = await _repositorio.ObtenerPorCodigoAsync(codigo);

        if (cupon == null)
            throw new KeyNotFoundException($"Cupón {codigo} no encontrado.");

        // Marcamos como usado
        cupon.MarcarComoUsado(usuarioId);

        await _repositorio.ActualizarAsync(cupon);
    }
}
