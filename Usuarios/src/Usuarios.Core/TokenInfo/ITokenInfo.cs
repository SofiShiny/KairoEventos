using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usuarios.Core.TokenInfo
{
    public interface ITokenInfo
    {
        Task<string> ObtenerIdUsuarioToken();
        Task<string> ObtenerIdUsuarioDadoElToken(string token);
    }
}
