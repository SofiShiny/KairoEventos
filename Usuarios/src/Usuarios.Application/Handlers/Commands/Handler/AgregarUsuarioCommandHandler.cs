using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Usuarios.Application.Dtos;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;
using Usuarios.Application.Exceptions;
using Usuarios.Application.Handlers.Commands.Command;
using Usuarios.Application.Validators;
using Usuarios.Core.Repository;
using Usuarios.Core.Services;
using Usuarios.Core.TokenInfo;
using Usuarios.Domain.Entidades;

namespace Usuarios.Application.Handlers.Commands.Handler
{
    public class AgregarUsuarioCommandHandler : IRequestHandler<AgregarUsuarioCommand, AgregarUsuarioDto>
    {
        private IRepository _repository;
        private IValidator<AgregarUsuarioDto> _validator;
        private IMapper _mapper;
        private IAccesManagement<UsuarioKeycloak> _accesManagement;
        private ILogger<AgregarUsuarioCommandHandler> _logger;
        private ITokenInfo _tokenInfo;

        public AgregarUsuarioCommandHandler(IRepository repository, IValidator<AgregarUsuarioDto> validator,
            IMapper mapper, IAccesManagement<UsuarioKeycloak> accesManagement,
            ILogger<AgregarUsuarioCommandHandler> logger, ITokenInfo tokenInfo)
        {
            _accesManagement = accesManagement;
            _validator = validator;
            _logger = logger;
            _tokenInfo = tokenInfo;
            _mapper = mapper;
            _repository = repository;
        }
        /// <summary>
        /// Se necesita para validar y llamar a la infraestructura de la aplicacion, en vez de llamar a todos los servicios en la api
        /// <list type="number">
        /// <item><param name="request">La <em>solicitud</em> que contiene los <em>campos</em> del usuario a agregar</param></item>
        /// <item><param name="cancellationToken">El <em>token de cancelacion</em> del sistema</param></item>
        /// </list>
        /// </summary>
        /// <returns>El <strong>dto de agregar</strong>.</returns>
        public async Task<AgregarUsuarioDto> Handle(AgregarUsuarioCommand request, CancellationToken cancellationToken)
        {
            var id_token = await _tokenInfo.ObtenerIdUsuarioToken();

            _logger.LogInformation("Se obtuvo el id del usuario: {id}", id_token);

            var result = await _validator.ValidateAsync(request.AgregarUsuariotDto,cancellationToken);
            if (!result.IsValid)
            {
                var ex = new ValidatorException("Informacion no valida");
                ex.AgregarErrores(result.Errors);
                throw ex;
            }

            _logger.LogInformation("Datos Validos del usuario");

            var usuario = _mapper.Map<Usuario>(request.AgregarUsuariotDto);
            var usuarioKeycloak = _mapper.Map<UsuarioKeycloak>(usuario);
            usuarioKeycloak.Credentials = new List<Credenciales>()
                { new Credenciales() { Value = request.AgregarUsuariotDto.Contrasena } };

            _logger.LogInformation("Agregando usuario en el servicio Keycloak");

            var id = await _accesManagement.AgregarUsuario(usuarioKeycloak, request.AgregarUsuariotDto.Rol);

            _logger.LogInformation("El usuario {id} se agrego en keycloak por {id_token}", id, id_token);

            await _repository.AgregarUsuario(usuario,id);

            _logger.LogInformation("El usuario {id} se agrego en base de datos por {id_token}", id, id_token);

            return request.AgregarUsuariotDto;
        }
    }
}
