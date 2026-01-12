using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Usuarios.Domain.Entidades;

namespace Usuarios.Core.Repository
{
    public interface IRepositoryConsulta<T> where T : class
    {
        Task<IEnumerable<T>> ConsultarRegistros(string? busqueda);
    }
}
