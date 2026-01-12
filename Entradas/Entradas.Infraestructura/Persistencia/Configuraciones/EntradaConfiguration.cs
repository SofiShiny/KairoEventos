using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entradas.Dominio.Entidades;
using Entradas.Dominio.Enums;

namespace Entradas.Infraestructura.Persistencia.Configuraciones;

/// <summary>
/// Configuración de Entity Framework para la entidad Entrada
/// </summary>
public class EntradaConfiguration : IEntityTypeConfiguration<Entrada>
{
    public void Configure(EntityTypeBuilder<Entrada> builder)
    {
        // Configuración de tabla
        builder.ToTable("entradas", t =>
        {
            t.HasCheckConstraint("ck_entradas_monto_positivo", "monto > 0");
            t.HasCheckConstraint("ck_entradas_estado_valido", "estado IN (0, 1, 2, 3, 4)");
        });
        
        // Clave primaria
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedNever(); // El ID se genera en el dominio
        
        // Propiedades de EntidadBase
        builder.Property(e => e.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
            
        builder.Property(e => e.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .HasColumnType("timestamp with time zone")
            .IsConcurrencyToken()
            .IsRequired();
        
        // Propiedades específicas de Entrada
        builder.Property(e => e.EventoId)
            .HasColumnName("evento_id")
            .IsRequired();
            
        builder.Property(e => e.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();
            
        builder.Property(e => e.AsientoId)
            .HasColumnName("asiento_id")
            .IsRequired(false); // Nullable para entradas generales
            
        builder.Property(e => e.MontoOriginal)
            .HasColumnName("monto_original")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.MontoDescuento)
            .HasColumnName("monto_descuento")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.Monto)
            .HasColumnName("monto")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.CuponesAplicados)
            .HasColumnName("cupones_aplicados")
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(e => e.CodigoQr)
            .HasColumnName("codigo_qr")
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(e => e.Estado)
            .HasColumnName("estado")
            .HasConversion<int>() // Almacenar como entero
            .IsRequired();
            
        builder.Property(e => e.FechaCompra)
            .HasColumnName("fecha_compra")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // ==================== SNAPSHOT FIELDS (Desnormalización) ====================
        
        builder.Property(e => e.TituloEvento)
            .HasColumnName("titulo_evento")
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(e => e.ImagenEventoUrl)
            .HasColumnName("imagen_evento_url")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(e => e.Categoria)
            .HasColumnName("categoria")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(e => e.FechaEvento)
            .HasColumnName("fecha_evento")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(e => e.NombreSector)
            .HasColumnName("nombre_sector")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(e => e.Fila)
            .HasColumnName("fila")
            .HasMaxLength(10)
            .IsRequired(false);

        builder.Property(e => e.NumeroAsiento)
            .HasColumnName("numero_asiento")
            .IsRequired(false);
        
        // Índices
        builder.HasIndex(e => e.CodigoQr)
            .IsUnique()
            .HasDatabaseName("ix_entradas_codigo_qr");
            
        builder.HasIndex(e => e.EventoId)
            .HasDatabaseName("ix_entradas_evento_id");
            
        builder.HasIndex(e => e.UsuarioId)
            .HasDatabaseName("ix_entradas_usuario_id");
            
        builder.HasIndex(e => e.Estado)
            .HasDatabaseName("ix_entradas_estado");
            
        builder.HasIndex(e => e.FechaCompra)
            .HasDatabaseName("ix_entradas_fecha_compra");
        
        // Configuración de concurrencia optimista usando FechaActualizacion
        builder.Property(e => e.FechaActualizacion)
            .IsConcurrencyToken();
    }
}