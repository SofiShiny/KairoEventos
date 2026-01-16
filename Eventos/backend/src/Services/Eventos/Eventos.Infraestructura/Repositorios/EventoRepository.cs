using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.Enumeraciones;
using Eventos.Infraestructura.Persistencia;

namespace Eventos.Infraestructura.Repositorios;

public class EventoRepository : IRepositorioEvento
{
    private readonly EventosDbContext _context;
    private readonly ILogger<EventoRepository> _logger;

    public EventoRepository(EventosDbContext context, ILogger<EventoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Evento?> ObtenerPorIdAsync(Guid id, bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Obteniendo evento {EventoId}, AsNoTracking: {AsNoTracking}", id, asNoTracking);
        
        var query = _context.Eventos.Include(e => e.Asistentes);
        
        var evento = asNoTracking 
            ? await query.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            : await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        
        if (evento == null)
            _logger.LogDebug("Evento {EventoId} no encontrado", id);
        else
            _logger.LogDebug("Evento {EventoId} encontrado, estado: {Estado}, asistentes: {Asistentes}", 
                id, evento.Estado, evento.ConteoAsistentesActual);
            
        return evento;
    }

    public async Task<IEnumerable<Evento>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var eventos = await _context.Eventos
            .Include(e => e.Asistentes)
            .ToListAsync(cancellationToken);
        
        // DEBUG: Log de los valores leídos de la BD
        foreach (var e in eventos.Take(3))
        {
            Console.WriteLine($"[REPO] Evento {e.Id} leído de BD: EsVirtual={e.EsVirtual}, PrecioBase={e.PrecioBase}");
        }
        
        return eventos;
    }

    public async Task<IEnumerable<Evento>> ObtenerEventosPublicadosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Eventos
            .Include(e => e.Asistentes)
            .Where(e => e.Estado == EstadoEvento.Publicado)
            .OrderBy(e => e.FechaInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Evento>> ObtenerEventosPorOrganizadorAsync(string organizadorId, CancellationToken cancellationToken = default)
    {
        return await _context.Eventos
            .Include(e => e.Asistentes)
            .Where(e => e.OrganizadorId == organizadorId)
            .OrderByDescending(e => e.FechaInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(Evento evento, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Agregando evento {EventoId}", evento.Id);
        await _context.Eventos.AddAsync(evento, cancellationToken);
        var cambios = await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Evento {EventoId} agregado, {Cambios} cambios guardados", evento.Id, cambios);
    }

    public async Task ActualizarAsync(Evento evento, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Actualizando evento {EventoId}, estado: {Estado}, asistentes: {Asistentes}", 
            evento.Id, evento.Estado, evento.ConteoAsistentesActual);
        
        try
        {
            // Obtener los IDs de asistentes actuales en la base de datos
            var asistentesExistentesIds = await _context.Asistentes
                .Where(a => a.EventoId == evento.Id)
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);
            
            _logger.LogDebug("Asistentes existentes en BD: {Count}", asistentesExistentesIds.Count);
            
            // Verificar si la entidad ya está siendo rastreada
            var entry = _context.Entry(evento);
            _logger.LogDebug("Estado de tracking del evento: {Estado}", entry.State);
            
            // Si la entidad no está siendo rastreada, marcarla como modificada
            if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
            {
                _logger.LogDebug("Evento no está siendo rastreado, adjuntando al contexto");
                _context.Eventos.Update(evento);
            }
            else
            {
                _logger.LogDebug("Evento ya está siendo rastreado, detectando cambios explícitamente");
                // Forzar la detección de cambios en la colección de asistentes
                _context.ChangeTracker.DetectChanges();
            }
            
            // Procesar cada asistente para determinar si es nuevo o existente
            foreach (var asistente in evento.Asistentes)
            {
                var asistenteEntry = _context.Entry(asistente);
                _logger.LogDebug("Asistente {AsistenteId} estado inicial: {Estado}", asistente.Id, asistenteEntry.State);
                
                // Si el asistente NO existe en la BD, marcarlo como Added
                if (!asistentesExistentesIds.Contains(asistente.Id))
                {
                    _logger.LogDebug("Asistente {AsistenteId} es nuevo, marcando como Added", asistente.Id);
                    
                    if (asistenteEntry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                    {
                        _context.Asistentes.Add(asistente);
                    }
                    else if (asistenteEntry.State != Microsoft.EntityFrameworkCore.EntityState.Added)
                    {
                        asistenteEntry.State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    }
                }
                else
                {
                    _logger.LogDebug("Asistente {AsistenteId} ya existe en BD", asistente.Id);
                }
            }
            
            var cambios = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Evento {EventoId} actualizado exitosamente, {Cambios} cambios guardados", evento.Id, cambios);
            
            if (cambios == 0)
            {
                _logger.LogWarning("SaveChangesAsync retornó 0 cambios para evento {EventoId}. Esto puede indicar un problema de tracking.", evento.Id);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Error de concurrencia al actualizar evento {EventoId}", evento.Id);
            throw new InvalidOperationException($"El evento {evento.Id} fue modificado por otro proceso", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar evento {EventoId}", evento.Id);
            throw;
        }
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eliminando evento {EventoId}", id);
        var evento = await ObtenerPorIdAsync(id, asNoTracking: false, cancellationToken);
        if (evento != null)
        {
            _context.Eventos.Remove(evento);
            var cambios = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Evento {EventoId} eliminado, {Cambios} cambios guardados", id, cambios);
        }
        else
        {
            _logger.LogWarning("Intento de eliminar evento {EventoId} que no existe", id);
        }
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Eventos.AnyAsync(e => e.Id == id, cancellationToken);
    }
}
