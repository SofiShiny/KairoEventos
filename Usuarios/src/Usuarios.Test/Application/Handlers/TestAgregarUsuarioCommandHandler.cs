using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Formats.Asn1;
using Usuarios.Application.Dtos;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;
using Usuarios.Application.Exceptions;
using Usuarios.Application.Handlers;
using Usuarios.Application.Handlers.Commands;
using Usuarios.Application.Handlers.Commands.Command;
using Usuarios.Application.Handlers.Commands.Handler;
using Usuarios.Application.Mappers;
using Usuarios.Application.Validators;
using Usuarios.Core.Repository;
using Usuarios.Core.Services;
using Usuarios.Core.TokenInfo;
using Usuarios.Domain.Entidades;

namespace Usuarios.Test.Application.Handlers;

public class TestAgregarUsuarioCommandHandler : DataSetTestCommandHandlers
{
    private AgregarUsuarioCommand Command;
    private Mock<IMapper> Mapper;
    private Mock<IValidator<AgregarUsuarioDto>> Validator;
    private AgregarUsuarioCommandHandler handler;
    private AgregarUsuarioCommand Command_Invalido;
    private Mock<ILogger<AgregarUsuarioCommandHandler>> logger;
    private Mock<ITokenInfo> tokenInfo;
    private UsuarioKeycloak _keycloak;

    public TestAgregarUsuarioCommandHandler()
    {
        Mapper = new();
        Validator = new();

        Command = new()
        {
            AgregarUsuariotDto = new()
            {
                Username = "test_string",
                Nombre = "test_string",
                Correo = "testString@hotmail.com",
                Contrasena = "Test123456789*",
                ConfirmarContrasena = "Test123456789*",
                Telefono = "12345678910",
                Direccion = "test_string",
                Rol = "Organizador"
            }
        };

        Command_Invalido = new()
        {
            AgregarUsuariotDto = new()
            {
                Username = "test_string",
                Nombre = "test_string",
                Correo = "testString@hotmail.com",
                Contrasena = "Test123456789*",
                ConfirmarContrasena = "Test123456",
                Telefono = "12345678910",
                Direccion = "test_string",
                Rol = "Organizador"
            }
        };
        logger = new();
        tokenInfo = new();
        handler = new(repository.Object, Validator.Object, Mapper.Object, accesManagement.Object, logger.Object, tokenInfo.Object);
        _keycloak = new()
        {
            Username = "test",
            Email = "test@gmail.com",
            Attributes = new Atributos() { Address = new List<string>() { "test" }, Name = new List<string>() { "test" }, Number = new List<string>() { "12345678910" } },
            Credentials = new List<Credenciales>() { new() { Value = "Test123456789*" } }
        };
    }

    [Fact]
    public async Task Test_AgregarUsuarioCommandHandler_DevuelveElObjetoCorrecto_EqualTrue()
    {
        Validator.Setup(a => a.ValidateAsync(It.IsAny<AgregarUsuarioDto>(), CancellationToken.None)).ReturnsAsync(_result);
        accesManagement.Setup(r => r.AgregarUsuario(It.IsAny<UsuarioKeycloak>(), It.IsAny<string>())).ReturnsAsync(Guid.Empty);
        Mapper.Setup(m => m.Map<UsuarioKeycloak>(It.IsAny<Usuario>())).Returns(_keycloak);
        repository.Setup(r => r.AgregarUsuario(It.IsAny<Usuario>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
        accesManagement.Setup(r => r.AgregarUsuario(It.IsAny<UsuarioKeycloak>(), It.IsAny<string>())).ReturnsAsync(Guid.Empty);
        tokenInfo.Setup(t => t.ObtenerIdUsuarioToken()).ReturnsAsync(It.IsAny<string>());

        var usuarioDto = await handler.Handle(Command, CancellationToken.None);

        Assert.Equal(Command.AgregarUsuariotDto, usuarioDto);
    }

    [Fact]
    public async Task Test_AgregarUsuarioCommandHandler_ObjetoDtoInvalido_LanzaValidationException()
    {
        tokenInfo.Setup(t => t.ObtenerIdUsuarioToken()).ReturnsAsync(It.IsAny<string>());
        Validator.Setup(a => a.ValidateAsync(It.IsAny<AgregarUsuarioDto>(), CancellationToken.None)).ReturnsAsync(_resultInValid);
        accesManagement.Setup(r => r.AgregarUsuario(It.IsAny<UsuarioKeycloak>(), It.IsAny<string>())).ReturnsAsync(Guid.Empty);

        await Assert.ThrowsAsync<ValidatorException>(async () => await handler.Handle(Command_Invalido, CancellationToken.None));
    }
}

