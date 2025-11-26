using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Eventos.Dominio.Entidades;
using Eventos.Dominio.Enumeraciones;

namespace Eventos.Infraestructura.Persistencia.Configuraciones;

public class EventoConfiguration : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> builder)
    {
        builder.ToTable("Eventos");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Titulo)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Titulo");

        builder.Property(e => e.Descripcion)
            .IsRequired()
            .HasMaxLength(2000)
            .HasColumnName("Descripcion");

        builder.OwnsOne(e => e.Ubicacion, direccion =>
        {
            direccion.Property(l => l.NombreLugar).HasMaxLength(200).IsRequired().HasColumnName("Ubicacion_NombreLugar");
            direccion.Property(l => l.Direccion).HasMaxLength(300).IsRequired().HasColumnName("Ubicacion_Direccion");
            direccion.Property(l => l.Ciudad).HasMaxLength(100).IsRequired().HasColumnName("Ubicacion_Ciudad");
            direccion.Property(l => l.Region).HasMaxLength(100).HasColumnName("Ubicacion_Region");
            direccion.Property(l => l.CodigoPostal).HasMaxLength(20).HasColumnName("Ubicacion_CodigoPostal");
            direccion.Property(l => l.Pais).HasMaxLength(100).IsRequired().HasColumnName("Ubicacion_Pais");
        });

        builder.Property(e => e.FechaInicio).IsRequired().HasColumnName("FechaInicio");
        builder.Property(e => e.FechaFin).IsRequired().HasColumnName("FechaFin");
        builder.Property(e => e.MaximoAsistentes).IsRequired().HasColumnName("MaximoAsistentes");
        
        builder.Property(e => e.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnName("Estado");

        builder.Property(e => e.OrganizadorId)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("OrganizadorId");

        builder.Ignore(e => e.EventosDominio);

        builder.HasMany(e => e.Asistentes)
            .WithOne()
            .HasForeignKey(a => a.EventoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.FechaInicio);
        builder.HasIndex(e => e.Estado);
        builder.HasIndex(e => e.OrganizadorId);
    }
}
