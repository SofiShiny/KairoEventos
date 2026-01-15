using BloquesConstruccion.Aplicacion.Comandos;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Dominio.Entidades;
using Eventos.Aplicacion.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventos.Aplicacion.Comandos;

// Handler que procesa el comando de crear un evento
// Siguiendo CQRS, separamos la lógica de negocio del controlador
// para mantener responsabilidades únicas y facilitar testing
public class CrearEventoComandoHandler : IRequestHandler<CrearEventoComando, Resultado<EventoDto>>
{
    private readonly IRepositorioEvento _repositorioEvento;
    private readonly IValidator<CrearEventoComando> _validator;
    private readonly ILogger<CrearEventoComandoHandler> _logger;

    public CrearEventoComandoHandler(
        IRepositorioEvento repositorioEvento, 
        IValidator<CrearEventoComando> validator,
        ILogger<CrearEventoComandoHandler> logger)
    {
        _repositorioEvento = repositorioEvento;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Resultado<EventoDto>> Handle(CrearEventoComando request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando creación de evento: {Titulo}", request.Titulo);
        
        // Aunque el DTO tiene validaciones, validamos el comando
        // para asegurar reglas de negocio adicionales antes de tocar la base de datos
        var validation = _validator.Validate(request);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Validación fallida para evento: {Error}", validation.Errors.First().ErrorMessage);
            return Resultado<EventoDto>.Falla(validation.Errors.First().ErrorMessage);
        }

        try
        {
            var evento = CrearEvento(request);
            _logger.LogInformation("Evento creado en memoria con ID: {EventoId}", evento.Id);

            await _repositorioEvento.AgregarAsync(evento, cancellationToken);
            _logger.LogInformation("Evento {EventoId} agregado al repositorio exitosamente", evento.Id);

            // Mapeamos a DTO para no exponer la entidad de dominio directamente
            // y para mantener el control sobre que datos se devuelven al cliente
            return Resultado<EventoDto>.Exito(EventoDtoMapper.Map(evento));
        }
        catch (ArgumentException ex)
        {
            // Captura errores de validación del dominio (constructor de Evento)
            // y los devuelve como un resultado fallido en lugar de una excepción
            _logger.LogError(ex, "Error de validación al crear evento");
            return Resultado<EventoDto>.Falla(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear evento");
            return Resultado<EventoDto>.Falla($"Error inesperado: {ex.Message}");
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
            request.OrganizadorId,
            request.Categoria ?? "General",
            request.PrecioBase,
            request.EsVirtual);
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
