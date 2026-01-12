using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Streaming.Dominio.Entidades;
using Streaming.Dominio.Interfaces;

namespace Streaming.Infraestructura.Persistencia;

public class RepositorioTransmisiones : IRepositorioTransmisiones
{
    private readonly StreamingDbContext _context;

    public RepositorioTransmisiones(StreamingDbContext context)
    {
        _context = context;
    }

    public async Task<Transmision?> ObtenerPorEventoIdAsync(Guid eventoId)
    {
        return await _context.Transmisiones
            .FirstOrDefaultAsync(t => t.EventoId == eventoId);
    }

    public async Task AgregarAsync(Transmision transmision)
    {
        await _context.Transmisiones.AddAsync(transmision);
    }
}

public class UnitOfWork : IUnitOfWork
{
    private readonly StreamingDbContext _context;

    public UnitOfWork(StreamingDbContext context)
    {
        _context = context;
    }

    public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
