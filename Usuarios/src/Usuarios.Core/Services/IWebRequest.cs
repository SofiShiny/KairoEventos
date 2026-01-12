using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usuarios.Core.Services
{
    public interface IWebRequest
    {
        Task<string> GetAsync(string url, Dictionary<string, string> headers = null!);
        Task<string> PostAsync(string url, HttpContent? body, Dictionary<string, string> headers = null!);
        Task<string> PutAsync(string url, HttpContent? body, Dictionary<string, string> headers = null!);
        Task<string> DeleteAsync(string url, Dictionary<string, string> headers = null!);
    }
}
