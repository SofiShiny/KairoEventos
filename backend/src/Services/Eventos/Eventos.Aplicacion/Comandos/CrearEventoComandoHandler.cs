using BloquesConstruccion.Aplicacion.Comandos;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Dominio.Entidades;
using Eventos.Aplicacion.DTOs;
using FluentValidation;
using MediatR;

namespace Eventos.Aplicacion.Comandos;

// Handler que procesa el comando de crear un evento
// Siguiendo CQRS, separamos la lógica de negocio del controlador
// para mantener responsabilidades únicas y facilitar testing
public class CrearEventoComandoHandler : IRequestHandler<CrearEventoComando, Resultado<EventoDto>>
{
    private readonly IRepositorioEvento _repositorioEvento;
    private readonly IValidator<CrearEventoComando> _validator;

    public CrearEventoComandoHandler(IRepositorioEvento repositorioEvento, IValidator<CrearEventoComando> validator)
    {
        _repositorioEvento = repositorioEvento;
        _validator = validator;
    }

    public async Task<Resultado<EventoDto>> Handle(CrearEventoComando request, CancellationToken cancellationToken)
    {
        // Aunque el DTO tiene validaciones, validamos el comando
        // para asegurar reglas de negocio adicionales antes de tocar la base de datos
        var validation = _validator.Validate(request);
        if (!validation.IsValid)
            return Resultado<EventoDto>.Falla(validation.Errors.First().ErrorMessage);

        try
        {
            var evento = CrearEvento(request);

            await _repositorioEvento.AgregarAsync(evento, cancellationToken);

            // Mapeamos a DTO para no exponer la entidad de dominio directamente
            // y para mantener el control sobre que datos se devuelven al cliente
            return Resultado<EventoDto>.Exito(EventoDtoMapper.Map(evento));
        }
        catch (ArgumentException ex)
        {
            // Captura errores de validación del dominio (constructor de Evento)
            // y los devuelve como un resultado fallido en lugar de una excepción
            return Resultado<EventoDto>.Falla(ex.Message);
        }
    }

    // Es privado y estático porque no se necesita estado de la clase y facilita testing
    private static Evento CrearEvento(CrearEventoComando request)
    {
        var ubicacion = MapUbicacion(request.Ubicacion!);
        return new Evento(
            request.Titulo,
            request.Descripcion,
            ubicacion,
            request.FechaInicio,
            request.FechaFin,
            request.MaximoAsistentes,
            request.OrganizadorId);
    }

    // Aca se mapea manualmente para evitar dependencias de AutoMapper para mantener
    // un control explícito sobre la transformación y mejorar el rendimiento
    private static Ubicacion MapUbicacion(UbicacionDto dto) => new(
        dto.NombreLugar!,
        dto.Direccion!,
        dto.Ciudad!,
        dto.Region ?? string.Empty,
        dto.CodigoPostal ?? string.Empty,
        dto.Pais!);
}
