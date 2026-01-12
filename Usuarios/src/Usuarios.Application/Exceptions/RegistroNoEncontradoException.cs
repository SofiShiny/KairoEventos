using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usuarios.Application.Exceptions
{
    public class RegistroNoEncontradoException : Exception
    {
        public RegistroNoEncontradoException(string message) : base(message)
        {
        }
    }
}
