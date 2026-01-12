using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Usuarios.Application.Dtos;
using Usuarios.Application.Mappers;
using Usuarios.Domain.Entidades;

namespace Usuarios.Test.Application.Mappers;

public class TestMapperEliminarUsuarioDtoAUsuario
{
    private EliminarUsuarioDto _usuario;
    private IMapper _mapper;

    public TestMapperEliminarUsuarioDtoAUsuario()
    {
        _usuario = new()
        {
            Nombre = "test",
            Correo = "test@gmail.com",
            Telefono = "12345678910",
            Direccion = "test"
        };
        var configuracion = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), new NullLoggerFactory());
        _mapper = configuracion.CreateMapper();
    }

    [Fact]
    public void Test_MapEliminarUsuarioDtoAUsuario_TipoDeDatoCorrecto_Usuario()
    {
        Assert.IsType<Usuario>(_mapper.Map<Usuario>(_usuario));
    }
}