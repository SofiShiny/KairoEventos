using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Usuarios.Application.Dtos;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;
using Usuarios.Application.Exceptions;
using Usuarios.Application.Handlers.Commands;
using Usuarios.Application.Handlers.Commands.Command;
using Usuarios.Application.Validators;
using Usuarios.Core.Repository;
using Usuarios.Core.Services;
using Usuarios.Core.TokenInfo;
using Usuarios.Domain.Entidades;
using Usuarios.Domain.Enum;

namespace Usuarios.Application.Handlers.Commands.Handler;

public class RegistroTokenCommandHandler : IRequestHandler<RegistroTokenCommand, AgregarUsuarioDto>
{
    private readonly IValidator<RegistroTokenDto> _validator;
    private readonly ITokenInfo _tokenInfo;
    private readonly IRepositorioConsultaPorId<Usuario> _repositorioConsulta;
    private readonly IAccesManagement<UsuarioKeycloak> _accesManagement;
    private readonly IMapper _mapper;
    private readonly IRepository _repository;
    private readonly ILogger<RegistroTokenCommandHandler> _logger;
    public RegistroTokenCommandHandler(IValidator<RegistroTokenDto> validator, ITokenInfo tokenInfo,
        IRepositorioConsultaPorId<Usuario> repositorioConsulta, IAccesManagement<UsuarioKeycloak> accesManagement,
        IMapper mapper, IRepository repositorio, ILogger<RegistroTokenCommandHandler> logger)
    {
        _logger = logger;
        _validator = validator;
        _tokenInfo = tokenInfo;
        _accesManagement = accesManagement;
        _mapper = mapper;
        _repositorioConsulta = repositorioConsulta;
        _repository = repositorio;
    }
    /// <summary>
    /// Se necesita para validar y llamar a la infraestructura de la aplicacion, en vez de llamar a todos los servicios en la api
    /// <list type="number">
    /// <item><param name="request">La <em>solicitud</em> que contiene el <em>token</em> del usuario</param></item>
    /// <item><param name="cancellationToken">El <em>token de cancelacion</em> del sistema</param></item>
    /// </list>
    /// </summary>
    /// <returns>Un <strong>dto de agregar</strong>.</returns>
    public async Task<AgregarUsuarioDto> Handle(RegistroTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(request.TokenDto);
        if (!result.IsValid)
        {
            var ex = new ValidatorException("Informacion no valida");
            ex.AgregarErrores(result.Errors);
            throw ex;
        }
        _logger.LogInformation("Token valido");

        var id = await _tokenInfo.ObtenerIdUsuarioDadoElToken(request.TokenDto.Token);

        _logger.LogInformation("Se obtuvo el id del usuario que inicio sesion: {id}", id);

        var id_guid = Guid.Parse(id);
        try
        {
            _logger.LogInformation("Consultando el id del usuario a ver si se encuentra en la base de datos");
            await _repositorioConsulta.ConsultarPorId(id_guid);
        }
        catch (RegistroNoEncontradoException ex)
        {
            _logger.LogWarning(exception:ex,"Se lanzo una exception");

            var usuario = await _accesManagement.ConsultarUsuarioPorId(id);

            _logger.LogInformation("Usuario de keycloak: {correo}",usuario.Email);

            var usuarioEntidad = _mapper.Map<Usuario>(usuario);
            usuarioEntidad.Rol = Rol.Usuario;
            await _accesManagement.AsignarRol(usuarioEntidad.Correo.Value,"Usuario");
            await _repository.AgregarUsuario(usuarioEntidad, id_guid);

            _logger.LogInformation("Usuario {id} agregado en base de datos por {usuario}",id_guid, "System");

            return _mapper.Map<AgregarUsuarioDto>(usuarioEntidad);
        }

        throw new InvalidOperationException("El usuario existe");
    }
}