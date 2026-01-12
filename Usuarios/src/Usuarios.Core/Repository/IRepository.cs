using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Usuarios.Domain.Entidades;

namespace Usuarios.Core.Repository
{
    public interface IRepository
    {
        Task AgregarUsuario(Usuario usuarioDto, Guid Id);
        Task ActualizarUsuario(Usuario usuarioDto, Guid Id);
        Task EliminarUsuario(Usuario usuarioDto, Guid Id);
    }
}
