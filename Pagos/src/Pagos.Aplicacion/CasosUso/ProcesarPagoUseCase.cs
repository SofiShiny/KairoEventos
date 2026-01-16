using MassTransit;
using Pagos.Aplicacion.Eventos;
using Pagos.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Pagos.Aplicacion.CasosUso;

public interface IProcesarPagoUseCase
{
    Task EjecutarAsync(Guid transaccionId, string tarjeta);
}

public class ProcesarPagoUseCase : IProcesarPagoUseCase
{
    private readonly IRepositorioTransacciones _repositorio;
    private readonly IPasarelaPago _pasarela;
    private readonly IGeneradorFactura _generadorFactura;
    private readonly IAlmacenadorArchivos _almacenador;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProcesarPagoUseCase> _logger;

    public ProcesarPagoUseCase(
        IRepositorioTransacciones repositorio,
        IPasarelaPago pasarela,
        IGeneradorFactura generadorFactura,
        IAlmacenadorArchivos almacenador,
        IPublishEndpoint publishEndpoint,
        ILogger<ProcesarPagoUseCase> logger)
    {
        _repositorio = repositorio;
        _pasarela = pasarela;
        _generadorFactura = generadorFactura;
        _almacenador = almacenador;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task EjecutarAsync(Guid transaccionId, string tarjeta)
    {
        var tx = await _repositorio.ObtenerPorIdAsync(transaccionId);
        if (tx == null) return;

        _logger.LogInformation("Iniciando procesamiento de pago para Tx: {TxId}", transaccionId);

        // TC-051: Si esto lanza excepción de red, Hangfire reintenta automáticamente
        var resultado = await _pasarela.CobrarAsync(tx.Monto, tarjeta);

        if (resultado.Exitoso)
        {
            // TC-050: Proceso de éxito
            var pdf = _generadorFactura.GenerarPdf(tx);
            var url = await _almacenador.GuardarAsync($"factura_{tx.Id}.pdf", pdf);
            
            tx.Aprobar(url);
            await _repositorio.ActualizarAsync(tx);

            await _publishEndpoint.Publish(new PagoAprobadoEvento(
                tx.Id, tx.OrdenId, tx.UsuarioId, tx.Monto, url));
            
            _logger.LogInformation("Pago TX {TxId} aprobado exitosamente", transaccionId);
        }
        else
        {
            tx.Rechazar(resultado.MotivoRechazo ?? "Rechazado por pasarela");
            await _repositorio.ActualizarAsync(tx);

            await _publishEndpoint.Publish(new PagoRechazadoEvento(
                tx.Id, tx.OrdenId, tx.UsuarioId, tx.MensajeError!));
            
            _logger.LogWarning("Pago TX {TxId} rechazado: {Motivo}", transaccionId, tx.MensajeError);
        }
    }
}
