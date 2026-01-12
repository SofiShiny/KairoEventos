using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usuarios.Application.Dtos
{
    public class AgregarUsuarioDto
    {
        public required string Username { get; set; }
        public required string Nombre { get; set; }
        public required string Correo { get; set; }
        public required string Contrasena { get; set; }
        public required string ConfirmarContrasena { get; set; }
        public required string Telefono { get; set; }
        public required string Direccion { get; set; }
        public required string Rol { get; set; }
    }
}
