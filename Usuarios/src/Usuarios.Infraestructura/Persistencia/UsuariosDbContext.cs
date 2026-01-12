using Microsoft.EntityFrameworkCore;
using Usuarios.Dominio.Entidades;
using Usuarios.Infraestructura.Persistencia.Configuraciones;

namespace Usuarios.Infraestructura.Persistencia
{
    public class UsuariosDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        
        public UsuariosDbContext(DbContextOptions<UsuariosDbContext> options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfiguration(new UsuarioEntityConfiguration());
        }
    }
}
