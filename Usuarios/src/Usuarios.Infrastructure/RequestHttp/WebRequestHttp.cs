using Microsoft.Extensions.Logging;
using Usuarios.Core.Services;

namespace Usuarios.Infrastructure.RequestHttp
{
    public class WebRequestHttp : IWebRequest
    {
        private HttpClient _httpClient;
        private ILogger<WebRequestHttp> _logger;

        public WebRequestHttp(HttpClient httpClient, ILogger<WebRequestHttp> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
        }
        /// <summary>
        /// Se necesita para centralizar la asignacion de los headers a las peticiones, asi no se repite codigo.
        /// <list type="number">
        /// <item><param name="headers">El <em>id</em> del usuario.</param></item>
        /// </list>
        /// </summary>
        private void ValidarHeaders(Dictionary<string, string>? headers)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            if (headers != null && headers.Count > 0)

                foreach (var header in headers)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
        }
        /// <summary>
        /// Se necesita para centralizar el envio de llamadas http de Delete, asi no se repite codigo y podemos mockear IWebRequest.
        /// <list type="number">
        /// <item><param name="url">El <em>url</em> hacia la cual se le va hacer la peticion.</param></item>
        /// <item><param name="headers">Los <em>headers</em> necesarios para la peticion.</param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Contenido</strong> de la respuesta.</returns>
        public async Task<string> DeleteAsync(string url, Dictionary<string, string> headers = null!)
        {
            ValidarHeaders(headers);

            _logger.LogInformation("Enviamos solicitud de eliminar mediante HttpClient a la url {url}", url);

            var response = await  _httpClient.DeleteAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// Se necesita para centralizar el envio de llamadas http de Get, asi no se repite codigo y podemos mockear IWebRequest.
        /// <list type="number">
        /// <item><param name="url">El <em>url</em> hacia la cual se le va hacer la peticion.</param></item>
        /// <item><param name="headers">Los <em>headers</em> necesarios para la peticion.</param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Contenido</strong> de la respuesta.</returns>
        public async Task<string> GetAsync(string url, Dictionary<string, string> headers = null!)
        {
            ValidarHeaders(headers);

            _logger.LogInformation("Enviamos solicitud de Consultar mediante HttpClient a la url {url}", url);

            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// Se necesita para centralizar el envio de llamadas http de Post, asi no se repite codigo y podemos mockear IWebRequest.
        /// <list type="number">
        /// <item><param name="url">El <em>url</em> hacia la cual se le va hacer la peticion.</param></item>
        /// <item><param name="headers">Los <em>headers</em> necesarios para la peticion.</param></item>
        /// <item><param name="body">El <em>contenido</em> de la peticion.</param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Contenido</strong> de la respuesta.</returns>
        public async Task<string> PostAsync(string url, HttpContent? body, Dictionary<string, string> headers = null!)
        {
            ValidarHeaders(headers);

            _logger.LogInformation("Enviamos solicitud de Agregar mediante HttpClient a la url {url}", url);

            var response = await _httpClient.PostAsync(url, body);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// Se necesita para centralizar el envio de llamadas http de Put, asi no se repite codigo y podemos mockear IWebRequest.
        /// <list type="number">
        /// <item><param name="url">El <em>url</em> hacia la cual se le va hacer la peticion.</param></item>
        /// <item><param name="headers">Los <em>headers</em> necesarios para la peticion.</param></item>
        /// <item><param name="body">El <em>contenido</em> de la peticion.</param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Contenido</strong> de la respuesta.</returns>
        public async Task<string> PutAsync(string url, HttpContent? body, Dictionary<string, string> headers = null!)
        {
            ValidarHeaders(headers);

            _logger.LogInformation("Enviamos solicitud de Agregar mediante HttpClient a la url {url}", url);

            var response = await _httpClient.PutAsync(url, body);

            response.EnsureSuccessStatusCode();


            return await response.Content.ReadAsStringAsync();
        }
    }
}
