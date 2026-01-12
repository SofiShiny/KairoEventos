using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usuarios.Application.Dtos
{
    public class ActualizarUsuarioDto
    {
        public required string Nombre { get; set; }
        public required string Correo { get; set; }
        public required string Telefono { get; set; }
        public required string Direccion { get; set; }
    }
}
