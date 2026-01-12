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

public class TestConsultarUsuarioPorIdQueryHandler
{
    private ConsultarUsuarioPorIdQuery query;
    private Mock<IRepositorioConsultaPorId<Usuario>> usuarioRepository;
    private Mock<IMapper> mapper;
    private ConsultarUsuarioPorIdQueryHandler handler;
    private Mock<ILogger<ConsultarUsuarioPorIdQueryHandler>> logger;
    private Usuario usuarios;
    private ConsultarUsuarioDto dto;

    public TestConsultarUsuarioPorIdQueryHandler()
    {
        query = new()
        {
            IdUsuario = Guid.Empty
        };
        mapper = new();
        usuarioRepository = new();
        logger = new();
        handler = new(usuarioRepository.Object, mapper.Object, logger.Object);
        usuarios = new()
        {
            Username = "test",
            Nombre = "test",
            Correo = new Correo("test@gmail.com"),
            Telefono = "12345678910",
            Direccion = "test",
            Rol = Rol.Organizador,
            IsActive = true
        };
        dto = new()
        {
            Username = "test",
            Nombre = "test",
            Correo = "test@gmail.com",
            Telefono = "12345678910",
            Direccion = "test",
            Rol = "test",
            IsActive = true
        };
    }

    [Fact]
    public async Task Test_ConsultarUsuarioQueryHandler_ObjetoDeBusquedaValido_ListaVacia()
    {
        usuarioRepository.Setup(u => u.ConsultarPorId(It.IsAny<Guid>())).ReturnsAsync(usuarios);
        mapper.Setup(m => m.Map<ConsultarUsuarioDto>(It.IsAny<Usuario>())).Returns(dto);

        var usuario = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(usuario, dto);
    }

}