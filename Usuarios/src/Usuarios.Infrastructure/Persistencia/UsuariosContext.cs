using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Usuarios.Core.Database;
using Usuarios.Domain.Entidades;
using Usuarios.Domain.ObjetosValor;

namespace Usuarios.Infrastructure.Persistencia
{
    public class UsuariosContext(DbContextOptions<UsuariosContext> options) : DbContext(options), IContext<Usuario>
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.IdUsuario);
                entity.HasIndex(e => e.Username).IsUnique().HasFilter("\"IsActive\" = true");
                entity.Property(e => e.Username).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Correo).HasConversion(
                    c => c.Value,
                    value => new Correo(value)
                    ).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Correo).IsUnique().HasFilter("\"IsActive\" = true");
                entity.Property(e => e.Telefono).IsRequired().HasMaxLength(11);
                entity.Property(e => e.Direccion).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Rol).HasConversion<string>().IsRequired();
                entity.Property(e => e.IsActive).IsRequired();
                entity.ToTable(t => t.HasCheckConstraint("Check_rol", $"\"Rol\" in ('Organizador','Soporte','Usuario','Administrador')"));
            });
        }

        /// <summary>
        /// Se necesita para centralizar el llamado para guardar los cambios en base de datos, asi no repetir codigo y poder mockear IContext.
        /// </summary>
        public async Task Save()
        {
            await SaveChangesAsync();
        }
        
        /// <summary>
        /// Se necesita para centralizar la accion de agregar usuario en la base de datos, asi no repetir codigo y poder mockear IContext.
        /// <list type="number">
        /// <item><param name="registro">El <em>usuario</em> a registrar.</param></item>
        /// </list>
        /// </summary>
        public async Task Agregar(Usuario registro)
        {
            await Usuarios.AddAsync(registro);
        }

        /// <summary>
        /// Se necesita para centralizar la accion de consultar los usuarios de la base de datos, asi no repetir codigo y poder mockear IContext.
        /// <list type="number">
        /// <item><param name="busqueda">El <em>string de busqueda</em> del usuario.</param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Coleccion de usuarios</strong>.</returns>
        public async Task<IEnumerable<Usuario>> ToListAsync(string? busqueda)
        {
            if (busqueda == null)
            {
                return Usuarios.Where(u => u.IsActive == true);
            }

            return Usuarios.
                Where(u => ((u.Username.Contains(busqueda!) ||
                             u.Correo.Value.Contains(busqueda!) ||
                             u.Nombre.Contains(busqueda!) ||
                             u.Direccion.Contains(busqueda!) ||
                             u.Telefono.Contains(busqueda!)) && u.IsActive == true));
        }
        /// <summary>
        /// Se necesita para centralizar la accion de actualizar los usuarios de la base de datos, asi no repetir codigo y poder mockear IContext.
        /// <list type="number">
        /// <item><param name="busqueda">El <em>usuario</em> a modificar.</param></item>
        /// </list>
        /// </summary>
        public Task Actualizar(Usuario registro)
        {
            Usuarios.Attach(registro);
            Entry(registro).State = EntityState.Modified;
            Entry(registro).Property(u => u.Username).IsModified = false;
            Entry(registro).Property(u => u.Rol).IsModified = false;
            Entry(registro).Property(u => u.IdUsuario).IsModified = false;
            return Task.CompletedTask;
        }
        /// <summary>
        /// Se necesita para centralizar la accion de consultar por id los usuarios de la base de datos, asi no repetir codigo y poder mockear IContext.
        /// <list type="number">
        /// <item><param name="busqueda">El <em>Model Builder</em> el cual toma </param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Id</strong> del usuario registrado.</returns>
        public async Task<Usuario?> FindAsync(Guid idUsuario)
        {
            return await Usuarios.AsNoTracking().FirstOrDefaultAsync(u => (u.IdUsuario == idUsuario && u.IsActive == true));
        }
        /// <summary>
        /// Se necesita para centralizar la accion de establecer el registro en base de datos como Inactivo, asi no repetir codigo y poder mockear IContext.
        /// <list type="number">
        /// <item><param name="registro">El <em>usuario</em> a modificar.</param></item>
        /// </list>
        /// </summary>
        public void Remove(Usuario registro)
        {
            Usuarios.Attach(registro);
            Entry(registro).Property(u => u.IsActive).IsModified = true;
        }

        public DbSet<Usuario> Usuarios { get; set; }

    }
}
