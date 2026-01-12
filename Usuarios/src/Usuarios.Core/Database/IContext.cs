using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Usuarios.Domain;
using Usuarios.Domain.Entidades;

namespace Usuarios.Core.Database
{
    public interface IContext<T> where T : class
    {
        Task Agregar(T registro);
        Task Save();
        Task<IEnumerable<T>> ToListAsync(string? busqueda);
        Task<Usuario?> FindAsync(Guid idUsuario);
        Task Actualizar(T registro);
        void Remove(T registro);
    }
}
