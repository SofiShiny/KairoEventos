using MediatR;
using Microsoft.Extensions.Logging;
using Usuarios.Aplicacion.DTOs;
using Usuarios.Dominio.Repositorios;

namespace Usuarios.Aplicacion.Consultas;

public class ConsultarUsuarioQueryHandler : IRequestHandler<ConsultarUsuarioQuery, UsuarioDto?>
{
    private readonly IRepositorioUsuarios _repositorio;
    private readonly ILogger<ConsultarUsuarioQueryHandler> _logger;

    public ConsultarUsuarioQueryHandler(
        IRepositorioUsuarios repositorio,
        ILogger<ConsultarUsuarioQueryHandler> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task<UsuarioDto?> Handle(
        ConsultarUsuarioQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consultando usuario: {UsuarioId}", query.UsuarioId);

        var usuario = await _repositorio.ObtenerPorIdAsync(query.UsuarioId, cancellationToken);

        if (usuario == null || !usuario.EstaActivo)
        {
            return null;
        }

        return new UsuarioDto
        {
            Id = usuario.Id,
            Username = usuario.Username,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo.Valor,
            Telefono = usuario.Telefono.Valor,
            Direccion = usuario.Direccion.Valor,
            Rol = usuario.Rol,
            FechaCreacion = usuario.FechaCreacion
        };
    }
}
