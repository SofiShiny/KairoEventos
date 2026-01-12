using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Usuarios.Application.Dtos;
using Usuarios.Application.Handlers;
using Usuarios.Application.Handlers.Commands;
using Usuarios.Application.Handlers.Commands.Command;
using Usuarios.Application.Handlers.Commands.Handler;
using Usuarios.Core.Repository;
using Usuarios.Core.Services;
using Usuarios.Core.TokenInfo;
using Usuarios.Domain.Entidades;

namespace Usuarios.Test.Application.Handlers;

public class TestEliminarUsuarioCommandHandler : DataSetTestCommandHandlers
{
    private EliminarUsuarioCommand command;
    private Mock<IMapper> mapper;
    private EliminarUsuarioCommandHandler handler;
    private Mock<ILogger<EliminarUsuarioCommandHandler>> logger;
    private Mock<ITokenInfo> tokenInfo;
    public TestEliminarUsuarioCommandHandler()
    {
        command = new()
        {
            EliminarUsuarioDto = new()
            {
                Nombre = "test",
                Correo = "test@gmail.co",
                Telefono = "12345678910",
                Direccion = "test"
            },
            Id = Guid.Empty
        };
        mapper = new Mock<IMapper>();
        logger = new();
        tokenInfo = new();
        handler = new EliminarUsuarioCommandHandler(repository.Object, mapper.Object, accesManagement.Object, logger.Object, tokenInfo.Object);
    }

    [Fact]
    public async Task Test_EliminarUsuarioCommandHandler_DevuelveElObjetoCorrecto_SeLlamoAlRepositorio()
    {
        tokenInfo.Setup(t => t.ObtenerIdUsuarioToken()).ReturnsAsync(It.IsAny<string>());
        accesManagement.Setup(a => a.EliminarUsuario(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        mapper.Setup(m => m.Map<Usuario>(It.IsAny<EliminarUsuarioDto>())).Returns(It.IsAny<Usuario>());
        repository.Setup(r => r.EliminarUsuario(It.IsAny<Usuario>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await handler.Handle(command, CancellationToken.None);

        repository.Verify(r => r.EliminarUsuario(It.IsAny<Usuario>(), Guid.Empty), Times.Once);
    }

    [Fact]
    public async Task Test_EliminarUsuarioCommandHandler_DevuelveElObjetoCorrecto_SeLlamoAKeycloak()
    {
        tokenInfo.Setup(t => t.ObtenerIdUsuarioToken()).ReturnsAsync(It.IsAny<string>());
        accesManagement.Setup(a => a.EliminarUsuario(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        mapper.Setup(m => m.Map<Usuario>(It.IsAny<EliminarUsuarioDto>())).Returns(It.IsAny<Usuario>());
        repository.Setup(r => r.EliminarUsuario(It.IsAny<Usuario>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await handler.Handle(command, CancellationToken.None);

        accesManagement.Verify(r => r.EliminarUsuario(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task Test_EliminarUsuarioCommandHandler_DevuelveElObjetoCorrecto_SeLlamoATokenInfo()
    {
        tokenInfo.Setup(t => t.ObtenerIdUsuarioToken()).ReturnsAsync(It.IsAny<string>());
        accesManagement.Setup(a => a.EliminarUsuario(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        mapper.Setup(m => m.Map<Usuario>(It.IsAny<EliminarUsuarioDto>())).Returns(It.IsAny<Usuario>());
        repository.Setup(r => r.EliminarUsuario(It.IsAny<Usuario>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);

        await handler.Handle(command, CancellationToken.None);

        tokenInfo.Verify(r => r.ObtenerIdUsuarioToken(), Times.Once);
    }
}