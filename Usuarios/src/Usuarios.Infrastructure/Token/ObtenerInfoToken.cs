using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Usuarios.Application.Exceptions;
using Usuarios.Core.TokenInfo;

namespace Usuarios.Infrastructure.Token
{
    public class ObtenerInfoToken : ITokenInfo
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<ObtenerInfoToken> _logger;
        public ObtenerInfoToken(IHttpContextAccessor contextAccessor, ILogger<ObtenerInfoToken> logger)
        {
            _contextAccessor = contextAccessor;
            _logger = logger;
        }
        /// <summary>
        /// Se necesita para centralizar en un servicio la obtencion del id del usuario que inicio sesion, asi no se repite codigo y podemos mockear ITokenInfo
        /// </summary>
        /// <returns><strong>Id</strong> del usuario.</returns>
        public async Task<string> ObtenerIdUsuarioToken()
        {
            var result = _contextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            if (!result) { throw new AutenticacionException("El usuario no esta autenticado"); }

            _logger.LogInformation("Header de Autenticacion obtenido: {header}", token);

            var jwt = token.First().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1];
            var handler = new JwtSecurityTokenHandler();
            var token_jwt = handler.ReadJwtToken(jwt);

            return token_jwt.Claims.FirstOrDefault(t => t.Type == "sub")!.Value;
        }
        /// <summary>
        /// Se necesita para centralizar en un servicio la obtencion del id del usuario que inicio sesion, asi no se repite codigo y podemos mockear ITokenInfo.
        /// <list type="number">
        /// <item><param name="token">El <em>token</em> de la peticion.</param></item>
        /// </list>
        /// </summary>
        /// <returns><strong>Id</strong> del usuario.</returns>
        public async Task<string> ObtenerIdUsuarioDadoElToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var token_jwt = handler.ReadJwtToken(token);

            _logger.LogInformation("Se accede al token y se le lee para sacar el id del usuario");

            return token_jwt.Claims.FirstOrDefault(t => t.Type == "sub")!.Value;
        }
    }
}
