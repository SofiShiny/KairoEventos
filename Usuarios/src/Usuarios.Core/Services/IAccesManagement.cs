using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Usuarios.Domain.Entidades;

namespace Usuarios.Core.Services
{
    public interface IAccesManagement<T> where T : class
    {
        public Task<Guid> AgregarUsuario(T usuario, string rol);
        public void SetToken(string token);
        public Task ModificarUsuario(T usuario, Guid id);
        public Task EliminarUsuario(Guid id);
        public Task<T> ConsultarUsuarioPorId(string id);
        public Task AsignarRol(string correoUsuario, string nombreRol);
    }
}
