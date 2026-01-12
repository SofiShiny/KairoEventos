using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Usuarios.API.Middlewares;
using Usuarios.Application.Exceptions;
using Usuarios.Core.Database;
using Usuarios.Core.Date;
using Usuarios.Domain.Entidades;
using Usuarios.Domain.Enum;
using Usuarios.Domain.ObjetosValor;
using Usuarios.Infrastructure.Repositorios;

namespace Usuarios.Test.Infrastructure.Repositorios;

public class TestUsuarioRepositorio
{
    private Usuario usuario;
    private Mock<IContext<Usuario>> context;
    private Mock<IDateTime> dateTime;
    private UsuarioRepositorio repositorio;
    private IEnumerable<Usuario> usuarios;
    private Mock<ILogger<UsuarioRepositorio>> logger;
    private IEnumerable<Usuario> usuariosVacio;
    public TestUsuarioRepositorio()
    {
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
        context = new();
        dateTime = new();
        logger = new();
        repositorio = new(context.Object,logger.Object);
        usuarios = new[] { usuario };
        usuariosVacio = new List<Usuario>();
    }

    [Fact]
    public async Task Test_AgregarUsuario_SeEjecuta_SeLlamoUnaVezContexto()
    {
        context.Setup(c => c.Agregar(It.IsAny<Usuario>())).Returns(Task.CompletedTask);

        await repositorio.AgregarUsuario(usuario, It.IsAny<Guid>());

        context.Verify(c => c.Agregar(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task Test_ConsultarUsuario_DevuelveUnRegistro_EqualTrueEntreListas()
    {
        context.Setup(c => c.ToListAsync(It.IsAny<string>())).ReturnsAsync(usuarios);

        var resultado = await repositorio.ConsultarRegistros(It.IsAny<string>());

        Assert.Equal(resultado, usuarios);
    }

    [Fact]
    public async Task Test_ConsultarUsuario_NoDevuelveRegistros_RetornaUnRegistroNoEncontradoException()
    {
        context.Setup(c => c.ToListAsync(It.IsAny<string>())).ReturnsAsync(usuariosVacio);

        var ex = await Record.ExceptionAsync(async () => await repositorio.ConsultarRegistros(It.IsAny<string>()));

        Assert.IsType<RegistroNoEncontradoException>(ex);
    }

    [Fact]
    public async Task Test_ActualizarUsuario_SeEjecuta_SeLlamoUnaVezContexto()
    {
        context.Setup(c => c.Actualizar(It.IsAny<Usuario>())).Returns(Task.CompletedTask);

        await repositorio.ActualizarUsuario(usuario, It.IsAny<Guid>());

        context.Verify(c => c.Actualizar(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task Test_ConsultarUsuarioPorId_DevuelveUnRegistro_EqualTrueEntreObjetos()
    {
        context.Setup(c => c.FindAsync(It.IsAny<Guid>())).ReturnsAsync(usuario);

        var resultado = await repositorio.ConsultarPorId(It.IsAny<Guid>());

        Assert.Equal(resultado, usuario);
    }

    [Fact]
    public async Task Test_ConsultarUsuarioPorId_NoDevuelveRegistro_RetornaUnRegistroNoEncontradoException()
    {
        context.Setup(c => c.FindAsync(It.IsAny<Guid>())).ReturnsAsync(It.IsAny<Usuario>());

        var ex = await Record.ExceptionAsync(async () => await repositorio.ConsultarPorId(It.IsAny<Guid>()));

        Assert.IsType<RegistroNoEncontradoException>(ex);
    }

    [Fact]
    public async Task Test_EliminarUsuario_SeEjecuta_SeLlamoAlContext()
    {
        context.Setup(c => c.Remove(It.IsAny<Usuario>()));

        await repositorio.EliminarUsuario(usuario, It.IsAny<Guid>());

        context.Verify(c => c.Remove(It.IsAny<Usuario>()), Times.Once);
    }

}