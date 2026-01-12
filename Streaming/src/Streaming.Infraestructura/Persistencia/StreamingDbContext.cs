using System;
using Microsoft.EntityFrameworkCore;
using Streaming.Dominio.Entidades;
using Streaming.Dominio.Modelos;

namespace Streaming.Infraestructura.Persistencia;

public class StreamingDbContext : DbContext
{
    public StreamingDbContext(DbContextOptions<StreamingDbContext> options) : base(options) { }

    public DbSet<Transmision> Transmisiones => Set<Transmision>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("streaming");

        modelBuilder.Entity<Transmision>(entity =>
        {
            entity.ToTable("transmisiones");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventoId).IsRequired();
            entity.HasIndex(e => e.EventoId).IsUnique();
            entity.Property(e => e.UrlAcceso).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Plataforma).HasConversion<int>();
            entity.Property(e => e.Estado).HasConversion<int>();
        });
    }
}
