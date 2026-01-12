using Microsoft.Extensions.Logging;
using Usuarios.Application.Exceptions;
using Usuarios.Core.Database;
using Usuarios.Core.Date;
using Usuarios.Core.Repository;
using Usuarios.Domain.Entidades;

namespace Usuarios.Infrastructure.Repositorios
{
    public class UsuarioRepositorio : IRepository, IRepositoryConsulta<Usuario>, IRepositorioConsultaPorId<Usuario>
    {
        private readonly IContext<Usuario> _context;
        private readonly ILogger<UsuarioRepositorio> _logger;
        public UsuarioRepositorio(IContext<Usuario> context, ILogger<UsuarioRepositorio> logger)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Se necesita un servicio de agregar en base de datos relacional, ya que, se pueden realizar bastantes acciones de este tipo y las base de datos transaccionales son especiales para estos casos.
        /// <list type="number">
        /// <item><param name="usuario">El <em>usuario</em> a agregar.</param></item>
        /// <item><param name="Id">El <em>id</em> del usuario.</param></item>
        /// </list>
        /// </summary>
        public async Task AgregarUsuario(Usuario usuario, Guid Id)
        {
            usuario.IdUsuario = Id;
            await _context.Agregar(usuario);
            await _context.Save();

            _logger.LogInformation("Guardado Exitoso en Base de Datos");
        }
        /// <summary>
        /// Se necesita un servicio de consultar en base de datos relacional, ya que, no estamos usando CQRS y usamos una base de datos relacional para las transacciones.
        /// <list type="number">
        /// <item><param name="Id">El <em>string de busqueda</em> del usuario.</param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Coleccion de Usuarios</strong> consultados.</returns>
        public async Task<IEnumerable<Usuario>> ConsultarRegistros(string? busqueda)
        {
            var usuarios = await _context.ToListAsync(busqueda);

            _logger.LogInformation("Consulta Realizada");

            if (!usuarios.Any())
            {
                throw new RegistroNoEncontradoException("Usuarios no encontrados");
            }
            return usuarios;
        }
        /// <summary>
        /// Se necesita un servicio de actualizar en base de datos relacional, ya que, se pueden realizar bastantes acciones de este tipo y las base de datos transaccionales son especiales para estos casos.
        /// <list type="number">
        /// <item><param name="usuario">El <em>usuario</em> a actualizar.</param></item>
        /// <item><param name="Id">El <em>id</em> del usuario.</param></item>
        /// </list>
        /// </summary>
        public async Task ActualizarUsuario(Usuario usuario, Guid Id)
        {
            usuario.IdUsuario = Id;
            await _context.Actualizar(usuario);
            await _context.Save();

            _logger.LogInformation("Actualizacion exitosa en base de datos");
        }
        /// <summary>
        /// Se necesita un servicio de consultar por id en base de datos relacional, ya que, no estamos usando CQRS y usamos una base de datos relacional para las transacciones.
        /// <list type="number">
        /// <item><param name="Id">El <em>id</em> del usuario.</param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Usuario</strong> consultado.</returns>
        public async Task<Usuario> ConsultarPorId(Guid id)
        {
            var usuario = await _context.FindAsync(id);

            _logger.LogInformation("Consulta por id realizada");

            return usuario ?? throw new RegistroNoEncontradoException("Usuario no encontrado");
        }
        /// <summary>
        /// Se necesita un servicio de eliminar en base de datos relacional, ya que, se pueden realizar bastantes acciones de este tipo y las base de datos transaccionales son especiales para estos casos.
        /// <list type="number">
        /// <item><param name="usuario">El <em>usuario</em> a eliminar.</param></item>
        /// <item><param name="Id">El <em>id</em> del usuario.</param></item>
        /// </list>
        /// </summary>
        public async Task EliminarUsuario(Usuario usuario, Guid Id)
        {
            usuario.IdUsuario = Id;
            usuario.IsActive = false;
            _context.Remove(usuario);
            await _context.Save();

            _logger.LogInformation("Usuario removido en base de datos");
        }
    }
}
