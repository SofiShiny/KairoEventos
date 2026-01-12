using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Usuarios.Infrastructure.Persistencia
{
    public class FactoryUsuarioContext : IDesignTimeDbContextFactory<UsuariosContext>
    {
        public UsuariosContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UsuariosContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=UsuariosDb;Username=postgres;Password=admin1234*");
            return new UsuariosContext(optionsBuilder.Options);
        }

    }
}
