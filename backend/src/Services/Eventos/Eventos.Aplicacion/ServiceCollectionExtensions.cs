using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
using AutoMapper;
using Eventos.Dominio.Entidades;
using Eventos.Aplicacion.DTOs;

namespace Eventos.Aplicacion;

public class EventoMappingProfile : Profile
{
    public EventoMappingProfile()
    {
        CreateMap<Evento, UbicacionDto>()
            .ForMember(d => d.NombreLugar, opt => opt.MapFrom(s => s.Ubicacion.NombreLugar))
            .ForMember(d => d.Direccion, opt => opt.MapFrom(s => s.Ubicacion.Direccion))
            .ForMember(d => d.Ciudad, opt => opt.MapFrom(s => s.Ubicacion.Ciudad))
            .ForMember(d => d.Region, opt => opt.MapFrom(s => s.Ubicacion.Region))
            .ForMember(d => d.CodigoPostal, opt => opt.MapFrom(s => s.Ubicacion.CodigoPostal))
            .ForMember(d => d.Pais, opt => opt.MapFrom(s => s.Ubicacion.Pais));

        CreateMap<Evento, EventoDto>()
            .ForMember(d => d.Ubicacion, opt => opt.MapFrom(s => s))
            .ForMember(d => d.Estado, opt => opt.MapFrom(s => s.Estado.ToString()))
            .ForMember(d => d.Asistentes, opt => opt.MapFrom(s => s.Asistentes.Select(a => new AsistenteDto { Id = a.Id, NombreUsuario = a.NombreUsuario, Correo = a.Correo, RegistradoEn = a.RegistradoEn })));
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventAplicacionServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}
