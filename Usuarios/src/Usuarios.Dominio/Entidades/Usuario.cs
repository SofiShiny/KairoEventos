using Usuarios.Dominio.Enums;
using Usuarios.Dominio.ObjetosValor;

namespace Usuarios.Dominio.Entidades
{
    public class Usuario
    {
        // Propiedades privadas
        public Guid Id { get; private set; }
        public string Username { get; private set; } = null!;
        public string Nombre { get; private set; } = null!;
        public Correo Correo { get; private set; } = null!;
        public Telefono Telefono { get; private set; } = null!;
        public Direccion Direccion { get; private set; } = null!;
        public Rol Rol { get; private set; }
        public bool EstaActivo { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime? FechaActualizacion { get; private set; }
        
        // Constructor privado para EF Core
        private Usuario() { }
        
        // Factory method para crear usuario
        public static Usuario Crear(
            string username,
            string nombre,
            Correo correo,
            Telefono telefono,
            Direccion direccion,
            Rol rol,
            Guid? id = null)
        {
            // Validaciones de negocio
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("El username no puede estar vacío");
            
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío");
            
            return new Usuario
            {
                Id = id ?? Guid.NewGuid(),
                Username = username,
                Nombre = nombre,
                Correo = correo,
                Telefono = telefono,
                Direccion = direccion,
                Rol = rol,
                EstaActivo = true,
                FechaCreacion = DateTime.UtcNow
            };
        }
        
        // Métodos de negocio
        public void Actualizar(
            string nombre,
            Telefono telefono,
            Direccion direccion)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío");
            
            Nombre = nombre;
            Telefono = telefono;
            Direccion = direccion;
            FechaActualizacion = DateTime.UtcNow;
        }
        
        public void CambiarRol(Rol nuevoRol)
        {
            Rol = nuevoRol;
            FechaActualizacion = DateTime.UtcNow;
        }
        
        public void Desactivar()
        {
            EstaActivo = false;
            FechaActualizacion = DateTime.UtcNow;
        }
        
        public void Reactivar()
        {
            EstaActivo = true;
            FechaActualizacion = DateTime.UtcNow;
        }
    }
}
