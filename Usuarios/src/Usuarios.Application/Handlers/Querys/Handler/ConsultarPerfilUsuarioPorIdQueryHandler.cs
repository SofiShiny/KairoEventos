using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Usuarios.Application.Dtos;
using Usuarios.Application.Handlers.Querys;
using Usuarios.Application.Handlers.Querys.Query;
using Usuarios.Core.Repository;
using Usuarios.Domain.Entidades;

namespace Usuarios.Application.Handlers.Querys.Handler;

public class ConsultarPerfilUsuarioPorIdQueryHandler : IRequestHandler<ConsultarPerfilUsuarioPorIdQuery, ConsultarPerfilUsuarioPorIdDto>
{
    private readonly IRepositorioConsultaPorId<Usuario> _usuarioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ConsultarPerfilUsuarioPorIdQueryHandler> _logger;
    public ConsultarPerfilUsuarioPorIdQueryHandler(IRepositorioConsultaPorId<Usuario> usuarioRepository, IMapper mapper,
        ILogger<ConsultarPerfilUsuarioPorIdQueryHandler> logger)
    {
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
        _logger = logger;
    }
    /// <summary>
    /// Se necesita para validar y llamar a la infraestructura de la aplicacion, en vez de llamar a todos los servicios en la api
    /// <list type="number">
    /// <item><param name="request">La <em>solicitud</em> que contiene el <em>id</em> del usuario</param></item>
    /// <item><param name="cancellationToken">El <em>token de cancelacion</em> del sistema</param></item>
    /// </list>
    /// </summary>
    /// <returns><strong>Dto de consulta de perfil</strong>.</returns>
    public async Task<ConsultarPerfilUsuarioPorIdDto> Handle(ConsultarPerfilUsuarioPorIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consultando el perfil del usuario por id: {id}",request.IdUsuario);

        var usuario = await _usuarioRepository.ConsultarPorId(request.IdUsuario);

        _logger.LogInformation("Usuario Encontrado");

        return _mapper.Map<ConsultarPerfilUsuarioPorIdDto>(usuario);
    }
}