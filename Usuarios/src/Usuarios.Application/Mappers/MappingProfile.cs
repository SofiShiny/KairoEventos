using System.Security.Cryptography;
using AutoMapper;
using Usuarios.Application.Dtos;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;
using Usuarios.Domain.Entidades;

namespace Usuarios.Application.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AgregarUsuarioDto, Usuario>();
            CreateMap<Usuario, ConsultarUsuarioDto>();
            CreateMap<Usuario, AgregarUsuarioDto>();
            CreateMap<ActualizarUsuarioDto, Usuario>();
            CreateMap<EliminarUsuarioDto, Usuario>();
            CreateMap<Usuario, EliminarUsuarioDto>();
            CreateMap<ConsultarUsuarioDto, EliminarUsuarioDto>();
            CreateMap<Usuario, ConsultarPerfilUsuarioPorIdDto>();
            CreateMap<Usuario, UsuarioKeycloak>()
                .ForMember(k => k.Email, m => m.MapFrom(u => u.Correo))
                .ForMember(k => k.Attributes, m => m.MapFrom(u => new Atributos()
                {
                    Address = new List<string>() { u.Direccion }, Name = new List<string>() { u.Nombre },
                    Number = new List<string>() { u.Telefono }
                }));
            CreateMap<UsuarioKeycloak, Usuario>()
                .ForMember(k => k.Correo, m => m.MapFrom(u => u.Email))
                .ForMember(k => k.Direccion, m => m.MapFrom(u => u.Attributes.Address[0]))
                .ForMember(k => k.Telefono , m => m.MapFrom(u => u.Attributes.Number[0]))
                .ForMember(k => k.Nombre, m => m.MapFrom(u => u.Attributes.Name[0]));
        }
    }
}
