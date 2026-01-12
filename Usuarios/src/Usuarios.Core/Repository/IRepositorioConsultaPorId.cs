using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usuarios.Core.Repository
{
    public interface IRepositorioConsultaPorId<T> where T : class
    {
        Task<T> ConsultarPorId(Guid id);
    }
}
