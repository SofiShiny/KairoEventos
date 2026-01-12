using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Usuarios.Domain.Enum;
using Usuarios.Domain.ObjetosValor;

namespace Usuarios.Domain.Entidades
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid IdUsuario { get; set; }
        public required string Username { get; set; }
        public required string Nombre { get; set; }
        public required Correo Correo { get; set; }
        public required string Telefono { get; set; }
        public required string Direccion { get; set; }
        public required Rol Rol { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
