using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Entradas.Dominio.Entidades;
using Entradas.Dominio.Enums;
using Entradas.Dominio.Interfaces;
using Entradas.Dominio.Excepciones;
using Entradas.Infraestructura.Persistencia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Entradas.Infraestructura.Repositorios;

/// <summary>
/// Implementación del repositorio de entradas usando Entity Framework Core
/// </summary>
public class RepositorioEntradas : IRepositorioEntradas
{
    private readonly EntradasDbContext _context;
    private readonly ILogger<RepositorioEntradas> _logger;

    public RepositorioEntradas(EntradasDbContext context, ILogger<RepositorioEntradas> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Entrada?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ArgumentException("ID requerido", nameof(id));
        return await _context.Entradas.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Entrada?> ObtenerPorCodigoQrAsync(string codigoQr, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(codigoQr)) throw new ArgumentException("Código QR requerido", nameof(codigoQr));
        return await _context.Entradas.FirstOrDefaultAsync(e => e.CodigoQr == codigoQr, cancellationToken);
    }

    public async Task<IEnumerable<Entrada>> ObtenerPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        if (usuarioId == Guid.Empty) throw new ArgumentException("ID del usuario requerido", nameof(usuarioId));
        return await _context.Entradas
            .Where(e => e.UsuarioId == usuarioId)
            .OrderByDescending(e => e.FechaCompra)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Entrada>> ObtenerPorEventoAsync(Guid eventoId, CancellationToken cancellationToken = default)
    {
        if (eventoId == Guid.Empty) throw new ArgumentException("ID del evento requerido", nameof(eventoId));
        return await _context.Entradas
            .Where(e => e.EventoId == eventoId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Entrada>> ObtenerPorEstadoAsync(EstadoEntrada estado, CancellationToken cancellationToken = default)
    {
        return await _context.Entradas
            .Where(e => e.Estado == estado)
            .ToListAsync(cancellationToken);
    }

    public async Task<Entrada> GuardarAsync(Entrada entrada, CancellationToken cancellationToken = default)
    {
        if (entrada == null) throw new ArgumentNullException(nameof(entrada));

        // Verificar si existe otra entrada con el mismo QR (independiente del ID)
        var entradaConMismoQr = await _context.Entradas
            .FirstOrDefaultAsync(e => e.CodigoQr == entrada.CodigoQr && e.Id != entrada.Id, cancellationToken);
        
        if (entradaConMismoQr != null)
        {
            throw new InvalidOperationException($"Ya existe una entrada con el código QR: {entrada.CodigoQr}");
        }

        var existe = await _context.Entradas.AnyAsync(e => e.Id == entrada.Id, cancellationToken);

        if (!existe)
        {
            await _context.Entradas.AddAsync(entrada, cancellationToken);
        }
        else
        {
            _context.Entradas.Update(entrada);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return entrada;
    }

    public async Task<bool> EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ArgumentException("ID no puede ser vacío", nameof(id));
        var entrada = await ObtenerPorIdAsync(id, cancellationToken);
        if (entrada == null) return false;

        _context.Entradas.Remove(entrada);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExisteCodigoQrAsync(string codigoQr, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(codigoQr)) throw new ArgumentException("Código QR requerido", nameof(codigoQr));
        return await _context.Entradas.AnyAsync(e => e.CodigoQr == codigoQr, cancellationToken);
    }

    public async Task<int> ContarPorEventoYEstadoAsync(Guid eventoId, EstadoEntrada estado, CancellationToken cancellationToken = default)
    {
        if (eventoId == Guid.Empty) throw new ArgumentException("ID requerido", nameof(eventoId));
        return await _context.Entradas.CountAsync(e => e.EventoId == eventoId && e.Estado == estado, cancellationToken);
    }

    public async Task<IEnumerable<Entrada>> ObtenerPendientesVencidasAsync(DateTime fechaLimite, CancellationToken cancellationToken = default)
    {
        try
        {
            var entradas = await _context.Entradas
                .Where(e => (e.Estado == EstadoEntrada.PendientePago || e.Estado == EstadoEntrada.Reservada) 
                        && e.FechaCreacion < fechaLimite)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Se encontraron {Cantidad} entradas pendientes vencidas", entradas.Count);
            return entradas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener entradas pendientes vencidas");
            throw;
        }
    }
    
    public async Task<Entrada?> ObtenerActivaPorAsientoAsync(Guid asientoId, CancellationToken cancellationToken = default)
    {
        return await _context.Entradas
            .Where(e => e.AsientoId == asientoId && e.Estado != EstadoEntrada.Cancelada)
            .OrderByDescending(e => e.FechaCreacion)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Entrada>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Entradas
            .OrderByDescending(e => e.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Entrada>> ObtenerPorOrdenIdAsync(Guid ordenId, CancellationToken cancellationToken = default)
    {
        if (ordenId == Guid.Empty) 
            throw new ArgumentException("OrdenId no puede ser vacío", nameof(ordenId));

        _logger.LogDebug("Buscando entradas para OrdenId: {OrdenId}", ordenId);

        // Estrategia: El OrdenId es el ID de la primera entrada creada en la compra múltiple
        // Buscamos por:
        // 1. La entrada con ese ID exacto
        // 2. Todas las entradas del mismo usuario, evento y creadas en el mismo momento (±5 segundos)
        
        var entradaPrincipal = await _context.Entradas
            .FirstOrDefaultAsync(e => e.Id == ordenId, cancellationToken);

        if (entradaPrincipal == null)
        {
            _logger.LogWarning("No se encontró entrada principal con OrdenId: {OrdenId}", ordenId);
            return new List<Entrada>();
        }

        // Buscar todas las entradas relacionadas (mismo usuario, evento, y tiempo cercano)
        var ventanaTiempo = TimeSpan.FromSeconds(5);
        var fechaInicio = entradaPrincipal.FechaCreacion.AddSeconds(-ventanaTiempo.TotalSeconds);
        var fechaFin = entradaPrincipal.FechaCreacion.AddSeconds(ventanaTiempo.TotalSeconds);

        var entradasRelacionadas = await _context.Entradas
            .Where(e => e.UsuarioId == entradaPrincipal.UsuarioId 
                     && e.EventoId == entradaPrincipal.EventoId
                     && e.FechaCreacion >= fechaInicio 
                     && e.FechaCreacion <= fechaFin)
            .OrderBy(e => e.FechaCreacion)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Se encontraron {Cantidad} entradas para OrdenId: {OrdenId}", 
            entradasRelacionadas.Count, ordenId);

        return entradasRelacionadas;
    }

    public async Task ActualizarRangoAsync(IEnumerable<Entrada> entradas, CancellationToken cancellationToken = default)
    {
        if (entradas == null || !entradas.Any())
        {
            _logger.LogWarning("Se intentó actualizar un rango vacío de entradas");
            return;
        }

        var listaEntradas = entradas.ToList();
        _logger.LogDebug("Actualizando {Cantidad} entradas en lote", listaEntradas.Count);

        try
        {
            _context.Entradas.UpdateRange(listaEntradas);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Se actualizaron exitosamente {Cantidad} entradas", listaEntradas.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar rango de {Cantidad} entradas", listaEntradas.Count);
            throw;
        }
    }
}
