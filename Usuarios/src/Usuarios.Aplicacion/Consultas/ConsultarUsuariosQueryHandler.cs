using MediatR;
using Microsoft.Extensions.Logging;
using Usuarios.Aplicacion.DTOs;
using Usuarios.Dominio.Repositorios;

namespace Usuarios.Aplicacion.Consultas;

public class ConsultarUsuariosQueryHandler : IRequestHandler<ConsultarUsuariosQuery, IEnumerable<UsuarioDto>>
{
    private readonly IRepositorioUsuarios _repositorio;
    private readonly ILogger<ConsultarUsuariosQueryHandler> _logger;

    public ConsultarUsuariosQueryHandler(
        IRepositorioUsuarios repositorio,
        ILogger<ConsultarUsuariosQueryHandler> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task<IEnumerable<UsuarioDto>> Handle(
        ConsultarUsuariosQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consultando usuarios activos");

        var usuarios = await _repositorio.ObtenerActivosAsync(cancellationToken);

        return usuarios.Select(usuario => new UsuarioDto
        {
            Id = usuario.Id,
            Username = usuario.Username,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo.Valor,
            Telefono = usuario.Telefono.Valor,
            Direccion = usuario.Direccion.Valor,
            Rol = usuario.Rol,
            FechaCreacion = usuario.FechaCreacion
        });
    }
}
