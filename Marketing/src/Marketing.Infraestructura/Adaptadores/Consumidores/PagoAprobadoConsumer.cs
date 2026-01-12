using MassTransit;
using Marketing.Aplicacion.CasosUso;
using Marketing.Aplicacion.EventosExternos;
using Microsoft.Extensions.Logging;

namespace Marketing.Infraestructura.Adaptadores.Consumidores;

public class PagoAprobadoConsumer : IConsumer<PagoAprobadoEvento>
{
    private readonly ConsumirCuponUseCase _useCase;
    private readonly ILogger<PagoAprobadoConsumer> _logger;

    public PagoAprobadoConsumer(ConsumirCuponUseCase useCase, ILogger<PagoAprobadoConsumer> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PagoAprobadoEvento> context)
    {
        var evento = context.Message;

        if (string.IsNullOrWhiteSpace(evento.CodigoCupon))
        {
            _logger.LogInformation("El pago {TxId} no utilizó cupón de descuento.", evento.TransaccionId);
            return;
        }

        try
        {
            _logger.LogInformation("Procesando consumo de cupón {Codigo} para el pago {TxId}", 
                evento.CodigoCupon, evento.TransaccionId);

            await _useCase.EjecutarAsync(evento.CodigoCupon, evento.UsuarioId);
            
            _logger.LogInformation("Cupón {Codigo} quemado exitosamente.", evento.CodigoCupon);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("El cupón {Codigo} reportado en el pago no existe en Marketing.", evento.CodigoCupon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al intentar quemar el cupón {Codigo}", evento.CodigoCupon);
            // Dependiendo de la política, podríamos relanzar para reintento
        }
    }
}
