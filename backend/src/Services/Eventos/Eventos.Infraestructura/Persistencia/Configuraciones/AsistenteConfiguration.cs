using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Eventos.Dominio.Entidades;

namespace Eventos.Infraestructura.Persistencia.Configuraciones;

public class AsistenteConfiguration : IEntityTypeConfiguration<Asistente>
{
    public void Configure(EntityTypeBuilder<Asistente> builder)
    {
        builder.ToTable("Asistentes");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EventoId).HasColumnName("EventoId");

        builder.Property(a => a.UsuarioId)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("UsuarioId");

        builder.Property(a => a.NombreUsuario)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("NombreUsuario");

        builder.Property(a => a.Correo)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("Correo");

        builder.Property(a => a.RegistradoEn).IsRequired().HasColumnName("RegistradoEn");


        builder.HasIndex(a => new { a.EventoId, a.UsuarioId }).IsUnique();
    }
}
