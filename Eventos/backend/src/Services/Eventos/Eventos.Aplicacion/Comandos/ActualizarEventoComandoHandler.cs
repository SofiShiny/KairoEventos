using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Dominio.Repositorios;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Dominio.Entidades;
using Eventos.Aplicacion.DTOs;
using MediatR;

namespace Eventos.Aplicacion.Comandos;

public class ActualizarEventoComandoHandler : IRequestHandler<ActualizarEventoComando, Resultado<EventoDto>>
{
    private readonly IRepositorioEvento _repositorioEvento;
    
    public ActualizarEventoComandoHandler(IRepositorioEvento repositorioEvento) => _repositorioEvento = repositorioEvento;

    public async Task<Resultado<EventoDto>> Handle(ActualizarEventoComando request, CancellationToken cancellationToken)
    {
        // Valida que el evento existe antes de intentar actualizar
        var evento = await _repositorioEvento.ObtenerPorIdAsync(request.EventoId, asNoTracking: false, cancellationToken);
        if (evento == null) return Resultado<EventoDto>.Falla("Evento no encontrado");
        
        // Se construye la ubicación separadamente, para poder actualizaciar parcialmente la ubicación
        var nuevaUbicacion = ConstruirUbicacion(evento, request.Ubicacion);
        
        Console.WriteLine($"[DEBUG] Handler recibiendo: EsVirtual={request.EsVirtual}, PrecioBase={request.PrecioBase}");

        //null coalescing (??) permite que solo se cambie lo que viene en el request
        // Si un campo es null, mantiene el valor actual del evento
        evento.Actualizar(
            request.Titulo ?? evento.Titulo,
            request.Descripcion ?? evento.Descripcion,
            nuevaUbicacion,
            request.FechaInicio ?? evento.FechaInicio,
            request.FechaFin ?? evento.FechaFin,
            request.MaximoAsistentes ?? evento.MaximoAsistentes,
            request.Categoria,
            request.PrecioBase,
            request.EsVirtual);

        Console.WriteLine($"[DEBUG] Entidad tras Actualizar(): EsVirtual={evento.EsVirtual}, PrecioBase={evento.PrecioBase}");
        
        await _repositorioEvento.ActualizarAsync(evento, cancellationToken);
        
        // Verificación rápida del estado tras el guardado
        var dbState = await _repositorioEvento.ObtenerPorIdAsync(evento.Id, asNoTracking: true, cancellationToken);
        Console.WriteLine($"[DEBUG] Estado final en DB para {evento.Id}: EsVirtual={dbState?.EsVirtual}");

        return Resultado<EventoDto>.Exito(EventoDtoMapper.Map(evento));
    }

    private static Ubicacion ConstruirUbicacion(Evento original, UbicacionDto? cambio) => cambio is null
        ? original.Ubicacion
        : new Ubicacion(
            cambio.NombreLugar ?? original.Ubicacion.NombreLugar,
            cambio.Direccion ?? original.Ubicacion.Direccion,
            cambio.Ciudad ?? original.Ubicacion.Ciudad,
            cambio.Region ?? original.Ubicacion.Region,
            cambio.CodigoPostal ?? original.Ubicacion.CodigoPostal,
            cambio.Pais ?? original.Ubicacion.Pais);
}
