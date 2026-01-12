using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Usuarios.Application.Dtos;
using Usuarios.Application.Exceptions;
using Usuarios.Application.Handlers.Commands;
using Usuarios.Core.Repository;
using Usuarios.Core.Services;
using Usuarios.Core.TokenInfo;
using Usuarios.Domain.Entidades;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;
using Usuarios.Application.Handlers.Commands.Command;

namespace Usuarios.Application.Handlers.Commands.Handler
{
    public class ActualizarUsuarioCommandHandler : IRequestHandler<ActualizarUsuarioCommand,ActualizarUsuarioDto>
    {
        private IRepository _repository;
        private IValidator<ActualizarUsuarioDto> _validator;
        private IMapper _mapper;
        private IAccesManagement<UsuarioKeycloak> _accesManagement;
        private ILogger<ActualizarUsuarioCommandHandler> _logger;
        private ITokenInfo _tokenInfo;
        public ActualizarUsuarioCommandHandler(IRepository repository,
            IValidator<ActualizarUsuarioDto> validator, IMapper mapper, IAccesManagement<UsuarioKeycloak> accesManagement, ILogger<ActualizarUsuarioCommandHandler> logger, ITokenInfo tokenInfo)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
            _accesManagement = accesManagement;
            _logger = logger;
            _tokenInfo = tokenInfo;
        }
        /// <summary>
        /// Se necesita para validar y llamar a la infraestructura de la aplicacion, en vez de llamar a todos los servicios en la api
        /// <list type="number">
        /// <item><param name="request">La <em>solicitud</em> que contiene los <em>campos</em> a actualizar del usuario y el <em>id</em></param></item>
        /// <item><param name="cancellationToken">El <em>token de cancelacion</em> del sistema</param></item>
        /// </list>
        /// </summary>
        /// <returns>El <strong>dto de actualizacion</strong>.</returns>
        public async Task<ActualizarUsuarioDto> Handle(ActualizarUsuarioCommand request, CancellationToken cancellationToken)
        {
            var id_token = await _tokenInfo.ObtenerIdUsuarioToken();

            _logger.LogInformation("Se obtuvo el id: {id}", id_token);

            var result = await _validator.ValidateAsync(request.ActualizarUsuarioDto,cancellationToken);
            if (!result.IsValid)
            {
                var ex = new ValidatorException("Informacion no valida");
                ex.Data["Errores"] = result.Errors;
                throw ex;
            }
            _logger.LogInformation("Datos Validos");
            var usuario = _mapper.Map<Usuario>(request.ActualizarUsuarioDto);
            var usuarioKeycloak = _mapper.Map<UsuarioKeycloak>(usuario);

            _logger.LogInformation("Modificando usuario en el servicio Keycloak");

            await _accesManagement.ModificarUsuario(usuarioKeycloak, request.Id);

            _logger.LogInformation("El usuario {request} se modifico en keycloak por {id_token}",request.Id,id_token);

            await _repository.ActualizarUsuario(usuario, request.Id);

            _logger.LogInformation("El usuario {request} se modifico en base de datos por {id_token}", request.Id, id_token);

            return request.ActualizarUsuarioDto;
        }
    }
}
