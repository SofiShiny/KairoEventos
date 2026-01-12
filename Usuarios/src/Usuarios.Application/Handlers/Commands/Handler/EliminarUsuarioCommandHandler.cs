using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Usuarios.Application.Dtos;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;
using Usuarios.Application.Handlers.Commands;
using Usuarios.Application.Handlers.Commands.Command;
using Usuarios.Core.Repository;
using Usuarios.Core.Services;
using Usuarios.Core.TokenInfo;
using Usuarios.Domain.Entidades;

namespace Usuarios.Application.Handlers.Commands.Handler
{
    public class EliminarUsuarioCommandHandler : IRequestHandler<EliminarUsuarioCommand>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAccesManagement<UsuarioKeycloak> _accesManagement;
        private readonly ILogger<EliminarUsuarioCommandHandler> _logger;
        private readonly ITokenInfo _tokenInfo;

        public EliminarUsuarioCommandHandler(IRepository repository, 
            IMapper mapper, IAccesManagement<UsuarioKeycloak> accesManagement, ILogger<EliminarUsuarioCommandHandler> logger, ITokenInfo tokenInfo)
        {
            _repository = repository;
            _mapper = mapper;
            _accesManagement = accesManagement;
            _logger = logger;
            _tokenInfo = tokenInfo;
        }
        /// <summary>
        /// Se necesita para validar y llamar a la infraestructura de la aplicacion, en vez de llamar a todos los servicios en la api
        /// <list type="number">
        /// <item><param name="request">La <em>solicitud</em> que contiene los <em>campos</em> del usuario a eliminar y el <em>id</em> del mismo</param></item>
        /// <item><param name="cancellationToken">El <em>token de cancelacion</em> del sistema</param></item>
        /// </list>
        /// </summary>
        public async Task Handle(EliminarUsuarioCommand request, CancellationToken cancellationToken)
        {
            var id_token = await _tokenInfo.ObtenerIdUsuarioToken();

            _logger.LogInformation("Se obtuvo el id del usuario: {id}", id_token);

            await _accesManagement.EliminarUsuario(request.Id);

            _logger.LogInformation("El usuario {id} se elimino de keycloak por {id_token}", request.Id, id_token);

            await _repository.EliminarUsuario(_mapper.Map<Usuario>(request.EliminarUsuarioDto),request.Id);

            _logger.LogInformation("El usuario {id} se elimino de base de datos por {id_token}", request.Id, id_token);
        }
    }
}
