using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Usuarios.API.Middlewares;
using Usuarios.Application.Dtos;
using Usuarios.Application.Handlers;
using Usuarios.Application.Handlers.Querys;
using Usuarios.Application.Handlers.Querys.Handler;
using Usuarios.Application.Handlers.Querys.Query;
using Usuarios.Core.Repository;
using Usuarios.Domain.Entidades;
using Usuarios.Domain.Enum;
using Usuarios.Domain.ObjetosValor;

namespace Usuarios.Test.Application.Handlers;

public class TestConsultarPerfilUsuarioPorIdQueryHandler
{
    private ConsultarPerfilUsuarioPorIdQuery query;
    private Mock<IRepositorioConsultaPorId<Usuario>> usuarioRepository;
    private Mock<IMapper> mapper;
    private ConsultarPerfilUsuarioPorIdQueryHandler handler;
    private Mock<ILogger<ConsultarPerfilUsuarioPorIdQueryHandler>> logger;
    private Usuario usuario;
    private ConsultarPerfilUsuarioPorIdDto consultarPerfil;

    public TestConsultarPerfilUsuarioPorIdQueryHandler()
    {
        query = new()
        {
            IdUsuario = Guid.Empty
        };
        mapper = new();
        usuarioRepository = new();
        logger = new();
        handler = new(usuarioRepository.Object, mapper.Object,logger.Object);
        usuario = new()
        {
            Username = "test",
            Nombre = "test",
            Correo= new Correo("test@gmail.com"),
            Telefono = "12345678910",
            Direccion = "test",
            Rol = Rol.Organizador,
            IsActive = true
        };
        consultarPerfil = new()
        {
            Correo = usuario.Correo.Value,
            Direccion = usuario.Direccion,
            Nombre = usuario.Nombre,
            Telefono = usuario.Telefono
        };
    }

    [Fact]
    public async Task Test_ConsultarUsuarioQueryHandler_ObjetoDeBusquedaValido_ListaVacia()
    {
        usuarioRepository.Setup(u => u.ConsultarPorId(It.IsAny<Guid>())).ReturnsAsync(usuario);
        mapper.Setup(m => m.Map<ConsultarPerfilUsuarioPorIdDto>(It.IsAny<Usuario>())).Returns(consultarPerfil);

        await handler.Handle(query, CancellationToken.None);

        usuarioRepository.Verify(r => r.ConsultarPorId(Guid.Empty), Times.Once);
    }
}