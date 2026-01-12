using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Usuarios.API.Middlewares;
using Usuarios.Application.Dtos;
using Usuarios.Application.Exceptions;
using Usuarios.Application.Handlers;
using Usuarios.Application.Handlers.Querys;
using Usuarios.Application.Handlers.Querys.Handler;
using Usuarios.Application.Handlers.Querys.Query;
using Usuarios.Application.Mappers;
using Usuarios.Application.Validators;
using Usuarios.Core.Repository;
using Usuarios.Domain.Entidades;
using Usuarios.Domain.Enum;
using Usuarios.Domain.ObjetosValor;

namespace Usuarios.Test.Application.Handlers;

public class TestConsultarUsuariosQueryHandler
{
    private ConsultarUsuariosQuery query_vacio;
    private ConsultarUsuariosQuery query;
    private Mock<IRepositoryConsulta<Usuario>> usuarioRepository;
    private Mock<IMapper> mapper;
    private Mock<IValidator<BusquedaUsuarioDto>> validator;
    private ConsultarUsuariosQueryHandler handler;
    private Mock<ILogger<ConsultarUsuariosQueryHandler>> logger;
    private List<Usuario> _usuarios;
    private ConsultarUsuarioDto dto;
    protected ValidationResult _result;
    protected ValidationResult _resultInValid;

    public TestConsultarUsuariosQueryHandler()
    {
        usuarioRepository = new();
        mapper = new();
        validator = new();
        logger = new();
        handler = new ConsultarUsuariosQueryHandler(usuarioRepository.Object, validator.Object, mapper.Object, logger.Object);
        query_vacio = new()
        {
            Busqueda = new()
            {
                Busqueda = null
            }
        };
        query = new()
        {
            Busqueda = new()
            {
                Busqueda = "*"
            }
        };
        _usuarios = new()
        {
            new Usuario()
            {
                Username = "test",
                Nombre = "test",
                Correo = new Correo("test@gmail.com"),
                Telefono = "12345678910",
                Direccion = "test",
                Rol = Rol.Organizador,
                IsActive = true
            }
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
        _result = new();
        var failures = new List<ValidationFailure>
        {
            new ("test", "test")
        };
        _resultInValid = new(failures);
    }

    [Fact]
    public async Task Test_ConsultarUsuarioQueryHandler_ObjetoDeBusquedaValido_EqualEntreListas()
    {
        validator.Setup(a => a.ValidateAsync(It.IsAny<BusquedaUsuarioDto>(), CancellationToken.None)).ReturnsAsync(_result);
        usuarioRepository.Setup(r => r.ConsultarRegistros(It.IsAny<string>())).ReturnsAsync(_usuarios);
        mapper.Setup(m => m.Map<ConsultarUsuarioDto>(It.IsAny<Usuario>())).Returns(dto);

        var usuarios = await handler.Handle(query_vacio, CancellationToken.None);

        Assert.Equal(usuarios.First(), dto);
    }

    [Fact]
    public async Task Test_ConsultarUsuarioQueryHandler_ObjetoDeBusquedaInvalido_RetornaValidationException()
    {
        validator.Setup(a => a.ValidateAsync(It.IsAny<BusquedaUsuarioDto>(), CancellationToken.None)).ReturnsAsync(_resultInValid);

        var ex = await Record.ExceptionAsync(async () => await handler.Handle(query_vacio, CancellationToken.None));

        Assert.IsType<ValidatorException>(ex);
    }
}