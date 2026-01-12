using Pagos.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Pagos.Aplicacion.Jobs;

public class ConciliacionJob
{
    private readonly IRepositorioTransacciones _repositorio;
    private readonly IPasarelaPago _pasarela;
    private readonly ILogger<ConciliacionJob> _logger;

    public ConciliacionJob(
        IRepositorioTransacciones repositorio, 
        IPasarelaPago pasarela,
        ILogger<ConciliacionJob> logger)
    {
        _repositorio = repositorio;
        _pasarela = pasarela;
        _logger = logger;
    }

    // TC-052: Job programado de conciliación
    public async Task EjecutarConciliacionDiariaAsync()
    {
        _logger.LogInformation("Iniciando Job de Conciliación Diaria...");
        
        var transaccionesLocales = await _repositorio.ObtenerTodasAsync();
        var movimientosExternos = await _pasarela.ObtenerMovimientosAsync();

        // Lógica simplificada de conciliación
        // En un escenario real, aquí se compararían IDs y montos
        foreach (var tx in transaccionesLocales.Where(t => t.Estado == Dominio.Modelos.EstadoTransaccion.Pendiente))
        {
            _logger.LogWarning("Transacción {Id} sigue pendiente tras conciliación. Revisar manualmente.", tx.Id);
        }

        _logger.LogInformation("Conciliación finalizada.");
    }
}
