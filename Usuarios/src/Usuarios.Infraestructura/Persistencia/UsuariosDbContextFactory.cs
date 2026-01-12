using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Usuarios.Infraestructura.Persistencia
{
    public class UsuariosDbContextFactory : IDesignTimeDbContextFactory<UsuariosDbContext>
    {
        public UsuariosDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UsuariosDbContext>();
            
            // Usar una cadena de conexión por defecto para migraciones
            optionsBuilder.UseNpgsql("Host=localhost;Database=kairo_usuarios;Username=postgres;Password=postgres");
            
            return new UsuariosDbContext(optionsBuilder.Options);
        }
    }
}
