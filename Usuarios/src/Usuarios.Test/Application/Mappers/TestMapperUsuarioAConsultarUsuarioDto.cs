using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Usuarios.Application.Dtos;
using Usuarios.Application.Mappers;
using Usuarios.Domain.Entidades;
using Usuarios.Domain.Enum;
using Usuarios.Domain.ObjetosValor;

namespace Usuarios.Test.Application.Mappers;

public class TestMapperUsuarioAConsultarUsuarioDto
{
    private Usuario _usuario;
    private IMapper _mapper;
    public TestMapperUsuarioAConsultarUsuarioDto()
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
    public void Test_MapUsuarioAConsultarUsuarioDto_TipoDeDatoDevueltoCorrecto_ConsultarUsuarioDto()
    {
        Assert.IsType<ConsultarUsuarioDto>(_mapper.Map<ConsultarUsuarioDto>(_usuario));
    }
}