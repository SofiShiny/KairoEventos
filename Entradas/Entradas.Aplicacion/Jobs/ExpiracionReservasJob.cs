using System;
using System.Threading.Tasks;
using Entradas.Dominio.Interfaces;
using Entradas.Dominio.Eventos;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Entradas.Aplicacion.Jobs;

public class ExpiracionReservasJob
{
    private readonly IRepositorioEntradas _repositorio;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ExpiracionReservasJob> _logger;
    private readonly IConfiguration _configuration;

    public ExpiracionReservasJob(
        IRepositorioEntradas repositorio,
        IPublishEndpoint publishEndpoint,
        ILogger<ExpiracionReservasJob> logger,
        IConfiguration configuration)
    {
        _repositorio = repositorio;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task EjecutarAsync()
    {
        _logger.LogInformation("Iniciando job de expiración de reservas...");

        var minutosExpiracion = Microsoft.Extensions.Configuration.ConfigurationBinder.GetValue<int>(_configuration, "ReglasNegocio:TiempoExpiracionMinutos", 15);   
        var fechaLimite = DateTime.UtcNow.AddMinutes(-minutosExpiracion);
        var pendientesVencidas = await _repositorio.ObtenerPendientesVencidasAsync(fechaLimite);

        int procesadas = 0;
        foreach (var entrada in pendientesVencidas)
        {
            try
            {
                _logger.LogInformation("Cancelando reserva vencida: {EntradaId}", entrada.Id);

                entrada.Cancelar();
                await _repositorio.GuardarAsync(entrada);

                // Publicar evento para que Asientos.API libere el lugar
                await _publishEndpoint.Publish(new ReservaCanceladaEvento(
                    entrada.Id,
                    entrada.AsientoId,
                    entrada.EventoId,
                    entrada.UsuarioId,
                    DateTime.UtcNow
                ));

                procesadas++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar cancelación de entrada {EntradaId}", entrada.Id);    
            }
        }

        if (procesadas > 0)
        {
            _logger.LogInformation("Job finalizado. Se cancelaron {Cantidad} reservas.", procesadas);        
        }
    }
}
