using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Usuarios.Application.Dtos;
using Usuarios.Core.Date;
using Usuarios.Core.Services;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;

namespace Usuarios.Infrastructure.Keycloak
{
    public class TokenBackGround : BackgroundService
    {
        private string? _token;
        private DateTime _dateToken;
        private readonly IWebRequest _webRequest;
        private readonly ILogger<TokenBackGround> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDateTime _dateTime;
        public TokenBackGround(IWebRequest webRequest, IConfiguration configuration, IServiceProvider serviceProvider,
            IDateTime dateTime, ILogger<TokenBackGround> logger)
        {
            _logger = logger;
            _webRequest = webRequest;
            _dateTime = dateTime;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }
        /// <summary>
        /// Se necesita para actualizar cada cierto tiempo el token admin de keycloak y poder realizar operaciones dentro de la plataforma
        /// <list type="number">
        /// <item><param name="stoppingToken">El <em>token de cancelacion</em> del sistema</param></item>
        /// </list>
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!(stoppingToken.IsCancellationRequested))
            {
                if (_token == null || (_dateTime.Now - _dateToken).TotalSeconds >= 100)
                {

                    _logger.LogInformation("Agrupamos la configuracion del cliente de keycloak para realizar la peticion");

                    var parametros = new Dictionary<string, string>
                    {
                        { "client_id", _configuration["AdminKeycloak:client_id"]!},
                        { "client_secret", _configuration["AdminKeycloak:client_secret"]!},
                        { "grant_type", _configuration["AdminKeycloak:grant_type"]!}
                    };
                    var content = new FormUrlEncodedContent(parametros);

                    var response = await _webRequest.PostAsync(
                        "http://localhost:8080/realms/myrealm/protocol/openid-connect/token", content);

                    _logger.LogInformation("Peticion de token admin exitosa");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var serviceAccesManagement = scope.ServiceProvider.GetRequiredService<IAccesManagement<UsuarioKeycloak>>();

                        _logger.LogInformation("Asignamos el token al servicio de keycloak");

                        _token = JsonDocument.Parse(response).RootElement.GetProperty("access_token").GetString();
                        serviceAccesManagement.SetToken(_token!);
                    }
                    _dateToken = _dateTime.Now;
                }
                await Task.Delay(100, stoppingToken);
            }

        }
    }
}
