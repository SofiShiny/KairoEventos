using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Usuarios.Dominio.Entidades;
using Usuarios.Dominio.ObjetosValor;
using Usuarios.Dominio.Repositorios;
using Usuarios.Infraestructura.Persistencia;

namespace Usuarios.Infraestructura.Repositorios
{
    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly UsuariosDbContext _context;
        private readonly ILogger<RepositorioUsuarios> _logger;
        
        public RepositorioUsuarios(
            UsuariosDbContext context,
            ILogger<RepositorioUsuarios> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<Usuario?> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Obteniendo usuario por ID: {UsuarioId}", id);
            
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
        
        public async Task<Usuario?> ObtenerPorUsernameAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Obteniendo usuario por username: {Username}", username);
            
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        }
        
        public async Task<Usuario?> ObtenerPorCorreoAsync(
            Correo correo,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Obteniendo usuario por correo: {Correo}", correo.Valor);
            
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo.Valor == correo.Valor, cancellationToken);
        }
        
        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Obteniendo todos los usuarios");
            
            return await _context.Usuarios.ToListAsync(cancellationToken);
        }
        
        public async Task<IEnumerable<Usuario>> ObtenerActivosAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Obteniendo usuarios activos");
            
            return await _context.Usuarios
                .Where(u => u.EstaActivo)
                .ToListAsync(cancellationToken);
        }
        
        public async Task AgregarAsync(
            Usuario usuario,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Agregando usuario: {UsuarioId}", usuario.Id);
            
            await _context.Usuarios.AddAsync(usuario, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Usuario agregado exitosamente: {UsuarioId}", usuario.Id);
        }
        
        public async Task ActualizarAsync(
            Usuario usuario,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Actualizando usuario: {UsuarioId}", usuario.Id);
            
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Usuario actualizado exitosamente: {UsuarioId}", usuario.Id);
        }
        
        public async Task<bool> ExisteUsernameAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Verificando existencia de username: {Username}", username);
            
            return await _context.Usuarios
                .AnyAsync(u => u.Username == username, cancellationToken);
        }
        
        public async Task<bool> ExisteCorreoAsync(
            Correo correo,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Verificando existencia de correo: {Correo}", correo.Valor);
            
            return await _context.Usuarios
                .AnyAsync(u => u.Correo.Valor == correo.Valor, cancellationToken);
        }
    }
}
