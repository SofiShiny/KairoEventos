using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Usuarios.Dominio.Entidades;

namespace Usuarios.Infraestructura.Persistencia.Configuraciones
{
    public class UsuarioEntityConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");
            
            builder.HasKey(u => u.Id);
            
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.HasIndex(u => u.Username)
                .IsUnique();
            
            builder.Property(u => u.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            
            // Value Object: Correo
            builder.OwnsOne(u => u.Correo, correo =>
            {
                correo.Property(c => c.Valor)
                    .HasColumnName("Correo")
                    .IsRequired()
                    .HasMaxLength(100);
                
                correo.HasIndex(c => c.Valor)
                    .IsUnique();
            });
            
            // Value Object: Telefono
            builder.OwnsOne(u => u.Telefono, telefono =>
            {
                telefono.Property(t => t.Valor)
                    .HasColumnName("Telefono")
                    .IsRequired()
                    .HasMaxLength(15);
            });
            
            // Value Object: Direccion
            builder.OwnsOne(u => u.Direccion, direccion =>
            {
                direccion.Property(d => d.Valor)
                    .HasColumnName("Direccion")
                    .IsRequired()
                    .HasMaxLength(200);
            });
            
            builder.Property(u => u.Rol)
                .IsRequired()
                .HasConversion<int>();
            
            builder.Property(u => u.EstaActivo)
                .IsRequired();
            
            builder.Property(u => u.FechaCreacion)
                .IsRequired();
            
            builder.Property(u => u.FechaActualizacion);
        }
    }
}
