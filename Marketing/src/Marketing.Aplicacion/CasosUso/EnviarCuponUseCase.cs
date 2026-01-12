using Marketing.Aplicacion.Interfaces;
using Marketing.Dominio.Eventos;

namespace Marketing.Aplicacion.CasosUso;

public class EnviarCuponUseCase
{
    private readonly IRepositorioCupones _repositorio;
    private readonly IEventoPublicador _publicador;

    public EnviarCuponUseCase(IRepositorioCupones repositorio, IEventoPublicador publicador)
    {
        _repositorio = repositorio;
        _publicador = publicador;
    }

    public async Task EjecutarAsync(string codigo, Guid usuarioId)
    {
        var cupon = await _repositorio.ObtenerPorCodigoAsync(codigo);
        
        if (cupon == null)
            throw new KeyNotFoundException($"El cupón con código {codigo} no existe.");

        // Lógica de dominio dentro de la entidad
        cupon.AsignarADestinatario(usuarioId);

        await _repositorio.ActualizarAsync(cupon);

        // Publicar evento para Notificaciones
        await _publicador.PublicarAsync(new CuponEnviadoEvento(
            cupon.Id,
            cupon.Codigo,
            usuarioId,
            cupon.Valor,
            cupon.TipoDescuento.ToString()
        ));
    }
}
