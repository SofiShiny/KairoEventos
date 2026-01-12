using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Usuarios.Application.Dtos;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;
using Usuarios.Application.Mappers;
using Usuarios.Domain.Entidades;
using Usuarios.Domain.Enum;
using Usuarios.Domain.ObjetosValor;

namespace Usuarios.Test.Application.Mappers;

public class TestMapperUsuarioAUsuarioKeycloak
{
    private Usuario _usuario;
    private IMapper _mapper;

    public TestMapperUsuarioAUsuarioKeycloak()
    {
        _usuario = new()
        {
            IdUsuario = Guid.Empty,
            Username = "test",
            Nombre = "test",
            Correo = new Correo("test@gmail.com"),
            Telefono = "12345678910",
            Direccion = "test",
            Rol = Rol.Organizador,
            IsActive = true
        };
        var configuracion = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), new NullLoggerFactory());
        _mapper = configuracion.CreateMapper();
    }

    [Fact]
    public void Test_MapEliminarUsuarioDtoAUsuario_TipoDeDatoCorrecto_Usuario()
    {
        Assert.IsType<UsuarioKeycloak>(_mapper.Map<UsuarioKeycloak>(_usuario));
    }
}