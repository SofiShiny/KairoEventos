using Microsoft.EntityFrameworkCore;
using Asientos.Dominio.Agregados;
using Asientos.Dominio.Entidades;
using BloquesConstruccion.Dominio;
using Asientos.Dominio.ObjetosDeValor;
using System;

namespace Asientos.Infraestructura.Persistencia;

public class AsientosDbContext : DbContext
{
 public AsientosDbContext(DbContextOptions<AsientosDbContext> options) : base(options) { }
 public DbSet<Asiento> Asientos => Set<Asiento>();
 public DbSet<MapaAsientos> Mapas => Set<MapaAsientos>();

 [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
 public string? CreatedConnectionString { get; set; }

 protected override void OnModelCreating(ModelBuilder modelBuilder)
 {
  modelBuilder.Ignore<EventoDominio>(); 
  modelBuilder.Entity<MapaAsientos>().Ignore(x => x.Eventos); 
  
  modelBuilder.Entity<Asiento>(b =>
  {
   b.ToTable("Asientos");
   b.HasKey(x => x.Id);
   
   b.Property(x => x.MapaId).IsRequired();
   b.Property(x => x.EventoId).IsRequired();
   b.Property(x => x.Fila).IsRequired();
   b.Property(x => x.Numero).IsRequired();
   b.Property(x => x.Reservado).IsRequired();
   b.Property(x => x.Pagado).IsRequired().HasDefaultValue(false);
   b.Property(x => x.UsuarioId).IsRequired(false);
   
   b.OwnsOne(x => x.Categoria, cat =>
   {
    cat.Property(c => c.Nombre).HasColumnName("CategoriaNombre").IsRequired();
    cat.Property(c => c.PrecioBase).HasColumnName("CategoriaPrecioBase");
    cat.Property(c => c.TienePrioridad).HasColumnName("CategoriaTienePrioridad").IsRequired();
   });
  });

  modelBuilder.Entity<MapaAsientos>(b =>
  {
   b.ToTable("Mapas");
   b.HasKey(x => x.Id);
   b.Property(x => x.EventoId).IsRequired();

   b.HasMany(m => m.Asientos)
    .WithOne()
    .HasForeignKey(a => a.MapaId)
    .OnDelete(DeleteBehavior.Cascade);
    
   b.Navigation(x => x.Asientos).HasField("_asientos").UsePropertyAccessMode(PropertyAccessMode.Field);

   b.OwnsMany(m => m.Categorias, cat => 
   { 
       cat.ToTable("MapasCategorias");
       cat.Property<Guid>("Id").ValueGeneratedOnAdd();
       cat.HasKey("Id");
       cat.Property(c => c.Nombre).IsRequired(); 
       cat.Property(c => c.PrecioBase); 
       cat.Property(c => c.TienePrioridad).IsRequired(); 
       cat.WithOwner().HasForeignKey("MapaId");
   });
   b.Navigation(m => m.Categorias).HasField("_categorias").UsePropertyAccessMode(PropertyAccessMode.Field);
  });
 }
}
