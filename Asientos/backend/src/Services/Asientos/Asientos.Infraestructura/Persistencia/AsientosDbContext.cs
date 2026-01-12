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
  modelBuilder.Ignore<EventoDominio>(); modelBuilder.Entity<MapaAsientos>().Ignore(x => x.Eventos); 
  
  modelBuilder.Entity<MapaAsientos>(b =>
  {
   b.ToTable("Mapas");
   b.HasKey(x => x.Id);
   b.Property(x => x.EventoId).IsRequired();
   b.Navigation(x => x.Asientos).UsePropertyAccessMode(PropertyAccessMode.Field);
   b.HasMany(x => x.Asientos).WithOne().HasForeignKey(x => x.MapaId).OnDelete(DeleteBehavior.Cascade);

   b.OwnsMany(m => m.Categorias, cat => { cat.Property(c => c.Nombre).IsRequired(); cat.Property(c => c.PrecioBase); cat.Property(c => c.TienePrioridad).IsRequired(); });
   b.Navigation(m => m.Categorias).UsePropertyAccessMode(PropertyAccessMode.Field);
  });

  modelBuilder.Entity<Asiento>(b =>
  {
   b.ToTable("Asientos");
   b.HasKey(x => x.Id);
   b.Property(x => x.MapaId).IsRequired();
   b.Property(x => x.EventoId).IsRequired();
   b.Property(x => x.Fila).IsRequired();
   b.Property(x => x.Numero).IsRequired();
   b.Property(x => x.Reservado).IsRequired();
   b.Property(x => x.UsuarioId).IsRequired(false); // Silla vacía por defecto
   
   b.OwnsOne(x => x.Categoria, cat =>
   {
    cat.Property(c => c.Nombre).HasColumnName("CategoriaNombre").IsRequired();
    cat.Property(c => c.PrecioBase).HasColumnName("CategoriaPrecioBase");
    cat.Property(c => c.TienePrioridad).HasColumnName("CategoriaTienePrioridad").IsRequired();
   });
   
  });
 }
}




