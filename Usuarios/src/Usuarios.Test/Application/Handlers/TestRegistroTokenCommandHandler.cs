using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Usuarios.API.Middlewares;
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
using Usuarios.Domain.Enum;
using Usuarios.Domain.ObjetosValor;

namespace Usuarios.Test.Application.Handlers;

public class TestRegistroTokenCommandHandler: DataSetTestCommandHandlers
{
    private RegistroTokenCommand Command;
    private Mock<IMapper> Mapper;
    private Mock<IValidator<RegistroTokenDto>> Validator;
    private RegistroTokenCommandHandler handler;
    private RegistroTokenCommand Command_Invalido;
    private Mock<ILogger<RegistroTokenCommandHandler>> logger;
    private Mock<ITokenInfo> tokenInfo;
    private Mock<IRepositorioConsultaPorId<Usuario>> repoConsultarId;
    private UsuarioKeycloak usuariokey;
    private string id;
    private Usuario usuario;
    private AgregarUsuarioDto dto;

    public TestRegistroTokenCommandHandler()
    {
        Mapper = new();
        Validator = new ();
        repoConsultarId = new();
        logger = new();
        Command = new()
        {
            TokenDto = new()
            {
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30"
            }
        };

        Command_Invalido = new()
        {
            TokenDto = new()
            {
                Token = "test"
            }
        };

        usuariokey = new()
        {
            Username = "test",
            Email = "test@gmail.com",
            Attributes = new Atributos() { Address = new List<string>() { "test" }, Name = new List<string>() { "test" }, Number = new List<string>() { "12345678910" } },
            Credentials = new List<Credenciales>() { new Credenciales() { Value = "Test123456789*" } }
        };

        logger = new();
        tokenInfo = new();
        handler = new(Validator.Object, tokenInfo.Object, repoConsultarId.Object, accesManagement.Object,Mapper.Object, repository.Object,logger.Object);
        id = Guid.NewGuid().ToString();
        usuario = new()
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
            Username = "test_string",
            Nombre = "test_string",
            Correo = "testString@hotmail.com",
            Contrasena = "Test123456789*",
            ConfirmarContrasena = "Test123456",
            Telefono = "12345678910",
            Direccion = "test_string",
            Rol = "Organizador"
        };
    }

    [Fact]
    public async Task Test_RegistroTokenCommandHandler_SeRegistraElUsuario_EqualTrue()
    {
        Validator.Setup(a => a.ValidateAsync(It.IsAny<RegistroTokenDto>(), CancellationToken.None)).ReturnsAsync(_result);
        Mapper.Setup(m => m.Map<Usuario>(It.IsAny<UsuarioKeycloak>())).Returns(usuario);
        Mapper.Setup(m => m.Map<AgregarUsuarioDto>(It.IsAny<Usuario>())).Returns(dto);
        repoConsultarId.Setup(r => r.ConsultarPorId(It.IsAny<Guid>())).ThrowsAsync(new RegistroNoEncontradoException("No se encontro el usuario"));
        accesManagement.Setup(r => r.ConsultarUsuarioPorId(It.IsAny<string>())).ReturnsAsync(usuariokey);
        accesManagement.Setup(r => r.AsignarRol(It.IsAny<string>(),It.IsAny<string>()));
        repository.Setup(r => r.AgregarUsuario(It.IsAny<Usuario>(), It.IsAny<Guid>()));
        tokenInfo.Setup(t => t.ObtenerIdUsuarioDadoElToken(It.IsAny<string>())).ReturnsAsync(id);

        var usuarioDto = await handler.Handle(Command, CancellationToken.None);

        Assert.Equal(dto.Correo, usuarioDto.Correo);
    }

    [Fact]
    public async Task Test_RegistroTokenCommandHandler_ObjetoDtoInvalido_LanzaValidationException()
    {
        Validator.Setup(a => a.ValidateAsync(It.IsAny<RegistroTokenDto>(), CancellationToken.None)).ReturnsAsync(_resultInValid);

        var ex = await Record.ExceptionAsync(async () => await handler.Handle(Command_Invalido, CancellationToken.None));

        Assert.IsType<ValidatorException>(ex);
    }
}