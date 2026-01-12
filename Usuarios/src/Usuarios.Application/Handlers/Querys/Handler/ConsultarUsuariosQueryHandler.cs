using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Usuarios.Application.Dtos;
using Usuarios.Application.Exceptions;
using Usuarios.Application.Handlers.Querys;
using Usuarios.Application.Handlers.Querys.Query;
using Usuarios.Core.Repository;
using Usuarios.Core.Services;
using Usuarios.Domain.Entidades;

namespace Usuarios.Application.Handlers.Querys.Handler
{
    public class ConsultarUsuariosQueryHandler : IRequestHandler<ConsultarUsuariosQuery, IEnumerable<ConsultarUsuarioDto>>
    {
        private readonly IRepositoryConsulta<Usuario> _usuarioRepository;
        private readonly IValidator<BusquedaUsuarioDto> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsultarUsuariosQueryHandler> _logger;

        public ConsultarUsuariosQueryHandler(IRepositoryConsulta<Usuario> usuarioRepository,
            IValidator<BusquedaUsuarioDto> validator,
            IMapper mapper, ILogger<ConsultarUsuariosQueryHandler> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _usuarioRepository = usuarioRepository;
            _validator = validator;
        }
        /// <summary>
        /// Se necesita para validar y llamar a la infraestructura de la aplicacion, en vez de llamar a todos los servicios en la api
        /// <list type="number">
        /// <item><param name="request">La <em>solicitud</em> que contiene el <em>string de busqueda</em> del usuario</param></item>
        /// <item><param name="cancellationToken">El <em>token de cancelacion</em> del sistema</param></item>
        /// </list>
        /// </summary>
        /// <returns>Una <strong>lista de dtos de consulta</strong>.</returns>
        public async Task<IEnumerable<ConsultarUsuarioDto>> Handle(ConsultarUsuariosQuery request, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request.Busqueda,cancellationToken);
            if (!result.IsValid)
            {
                var ex = new ValidatorException("Informacion no valida");
                ex.AgregarErrores(result.Errors);
                throw ex;
            }

            _logger.LogInformation("String de busqueda valido");

            var dict = await _usuarioRepository.ConsultarRegistros(request.Busqueda.Busqueda);

            _logger.LogInformation("Usuarios Encontrados");

            return dict.Select(u => _mapper.Map<ConsultarUsuarioDto>(u));
        }
    }
}
