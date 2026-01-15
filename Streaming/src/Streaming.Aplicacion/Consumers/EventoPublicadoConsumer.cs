using System;
using Eventos.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Streaming.Dominio.Entidades;
using Streaming.Dominio.Interfaces;
using Streaming.Dominio.Modelos;

namespace Streaming.Aplicacion.Consumers;

public class EventoPublicadoConsumer : IConsumer<EventoPublicadoEventoDominio>
{
    private readonly IRepositorioTransmisiones _repositorio;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EventoPublicadoConsumer> _logger;

    public EventoPublicadoConsumer(
        IRepositorioTransmisiones repositorio, 
        IUnitOfWork unitOfWork, 
        ILogger<EventoPublicadoConsumer> logger)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EventoPublicadoEventoDominio> context)
    {
        var mensaje = context.Message;
        _logger.LogInformation("Procesando evento publicado: {EventoId} - {Titulo}", mensaje.EventoId, mensaje.TituloEvento);

        if (!mensaje.EsVirtual)
        {
            _logger.LogInformation("El evento {EventoId} no es virtual, se ignora creación de transmisión.", mensaje.EventoId);
            return;
        }

        // Verificar si ya existe el streaming para evitar duplicados
        var existente = await _repositorio.ObtenerPorEventoIdAsync(mensaje.EventoId);
        if (existente != null)
        {
            _logger.LogWarning("La transmisión para el evento {EventoId} ya existe.", mensaje.EventoId);
            return;
        }

        // Crear nueva transmisión (Se asume Google Meet por defecto para TC-170)
        var transmision = Transmision.Crear(mensaje.EventoId, PlataformaTransmision.GoogleMeet);

        await _repositorio.AgregarAsync(transmision);
        await _unitOfWork.GuardarCambiosAsync();

        _logger.LogInformation("Transmisión generada para evento {EventoId}: {Url}", mensaje.EventoId, transmision.UrlAcceso);
    }
}
