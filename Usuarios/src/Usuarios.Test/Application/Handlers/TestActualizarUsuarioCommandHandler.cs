using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Usuarios.Application.Dtos;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;
using Usuarios.Application.Exceptions;
using Usuarios.Application.Handlers;
using Usuarios.Application.Handlers.Commands;
using Usuarios.Application.Handlers.Commands.Command;
using Usuarios.Application.Handlers.Commands.Handler;
using Usuarios.Application.Mappers;
using Usuarios.Application.Validators;
using Usuarios.Core.TokenInfo;
using Usuarios.Domain.Entidades;

namespace Usuarios.Test.Application.Handlers;

public class TestActualizarUsuarioCommandHandler: DataSetTestCommandHandlers
{
    private Mock<IValidator<ActualizarUsuarioDto>> actualizarUsuarioValidator;
    private Mock<IMapper> mapper;
    private ActualizarUsuarioCommandHandler _handler;
    private ActualizarUsuarioCommand Command;
    private ActualizarUsuarioCommand Command_Invalido;
    private Mock<ITokenInfo> _tokenInfo;
    private Mock<ILogger<ActualizarUsuarioCommandHandler>> _logger;
    public TestActualizarUsuarioCommandHandler() : base()
    {
        _tokenInfo = new Mock<ITokenInfo>();
        actualizarUsuarioValidator = new();
        mapper = new();
        _logger = new();
        _handler = new(repository.Object, actualizarUsuarioValidator.Object, mapper.Object, accesManagement.Object, _logger.Object, _tokenInfo.Object);
        Command = new ActualizarUsuarioCommand()
        {
            Id = Guid.NewGuid(),
            ActualizarUsuarioDto = new ActualizarUsuarioDto()
            {
                Nombre = "test_test",
                Correo = "test@gmail.com",
                Telefono = "12345678910",
                Direccion = "test_test"
            }
        };

        Command_Invalido = new ActualizarUsuarioCommand()
        {
            Id = Guid.NewGuid(),
            ActualizarUsuarioDto = new ActualizarUsuarioDto()
            {
                Nombre = "test",
                Correo = "test",
                Telefono = "12345678910",
                Direccion = "test"
            }
        };
    }
    [Fact]
    public async Task Test_ActualizarUsuarioCommandHandler_DevuelveElObjetoCorrecto_EqualTrue()
    {
        actualizarUsuarioValidator.Setup(a => a.ValidateAsync(It.IsAny<ActualizarUsuarioDto>(), CancellationToken.None)).ReturnsAsync(_result);
        _tokenInfo.Setup(t => t.ObtenerIdUsuarioToken()).ReturnsAsync(It.IsAny<string>());
        mapper.Setup(m => m.Map<Usuario>(It.IsAny<ActualizarUsuarioDto>())).Returns(It.IsAny<Usuario>());
        mapper.Setup(m => m.Map<UsuarioKeycloak>(It.IsAny<Usuario>())).Returns(It.IsAny<UsuarioKeycloak>());
        repository.Setup(r => r.ActualizarUsuario(It.IsAny<Usuario>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
        accesManagement.Setup(r => r.ModificarUsuario(It.IsAny<UsuarioKeycloak>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);

        var usuarioDto = await _handler.Handle(Command, CancellationToken.None);

        Assert.Equal(Command.ActualizarUsuarioDto, usuarioDto);
    }

    [Fact]
    public async Task Test_ActualizarUsuarioCommandHandler_ObjetoDtoInvalido_LanzaValidationException()
    {
        _tokenInfo.Setup(t => t.ObtenerIdUsuarioToken()).ReturnsAsync(It.IsAny<string>());
        actualizarUsuarioValidator.Setup(a => a.ValidateAsync(It.IsAny<ActualizarUsuarioDto>(), CancellationToken.None)).ReturnsAsync(_resultInValid);

        await Assert.ThrowsAsync<ValidatorException>(async () => await _handler.Handle(Command_Invalido, CancellationToken.None));
    }

    [Fact]
    public async Task Test_ActualizarUsuarioCommandHandler_NoEncuentraElTokenDelUsuario_LanzaValidationException()
    {
        _tokenInfo.Setup(l => l.ObtenerIdUsuarioToken()).Throws(new AutenticacionException("El usuario no esta autenticado"));

        await Assert.ThrowsAsync<AutenticacionException>(async () => await _handler.Handle(Command, CancellationToken.None));
    }
}
