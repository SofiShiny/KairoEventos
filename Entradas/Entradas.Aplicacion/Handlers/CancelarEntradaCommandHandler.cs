using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;
using Entradas.Dominio.Interfaces;
using Entradas.Dominio.Eventos;
using Entradas.Dominio.Excepciones;
using Entradas.Aplicacion.Comandos;

namespace Entradas.Aplicacion.Handlers;

/// <summary>
/// Handler para procesar la cancelación manual de una entrada
/// </summary>
public class CancelarEntradaCommandHandler : IRequestHandler<CancelarEntradaCommand, bool>
{
    private readonly IRepositorioEntradas _repositorio;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<CancelarEntradaCommandHandler> _logger;

    public CancelarEntradaCommandHandler(
        IRepositorioEntradas repositorio,
        IPublishEndpoint publisher,
        ILogger<CancelarEntradaCommandHandler> logger)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(CancelarEntradaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando cancelación de entrada {EntradaId} por el usuario {UsuarioId}", 
            request.EntradaId, request.UsuarioId);

        try
        {
            var entrada = await _repositorio.ObtenerPorIdAsync(request.EntradaId, cancellationToken);

            if (entrada == null)
            {
                _logger.LogWarning("Entrada {EntradaId} no encontrada", request.EntradaId);
                return false;
            }

            // Validación de seguridad: solo el dueño puede cancelar
            if (entrada.UsuarioId != request.UsuarioId)
            {
                _logger.LogWarning("El usuario {UsuarioId} no tiene permiso para cancelar la entrada {EntradaId}", 
                    request.UsuarioId, request.EntradaId);
                return false;
            }

            // Realizar cancelación en el dominio
            entrada.Cancelar();
            
            // Persistir cambios
            await _repositorio.GuardarAsync(entrada, cancellationToken);

            // Publicar evento de integración para que otros microservicios (Asientos) reaccionen
            await _publisher.Publish(new ReservaCanceladaEvento(
                entrada.Id,
                entrada.AsientoId,
                entrada.EventoId,
                entrada.UsuarioId,
                DateTime.UtcNow
            ), cancellationToken);

            _logger.LogInformation("Entrada {EntradaId} cancelada exitosamente y asiento liberado (si aplica). Reembolso simulado procesado.", entrada.Id);
            
            return true;
        }
        catch (DominioException ex)
        {
            _logger.LogWarning(ex, "Validación de dominio fallida al cancelar entrada {EntradaId}: {Message}", request.EntradaId, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al cancelar la entrada {EntradaId}", request.EntradaId);
            return false;
        }
    }
}
