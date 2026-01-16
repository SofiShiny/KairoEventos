using System;
using Microsoft.AspNetCore.Mvc;
using Streaming.Aplicacion.DTOs;
using Streaming.Dominio.Entidades;
using Streaming.Dominio.Interfaces;
using Streaming.Dominio.Modelos;

namespace Streaming.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StreamingController : ControllerBase
{
    private readonly IRepositorioTransmisiones _repositorio;
    private readonly IUnitOfWork _unitOfWork;

    public StreamingController(IRepositorioTransmisiones repositorio, IUnitOfWork unitOfWork)
    {
        _repositorio = repositorio;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("{eventoId:guid}")]
    public async Task<ActionResult<TransmisionDto>> ObtenerPorEvento(Guid eventoId)
    {
        var transmision = await _repositorio.ObtenerPorEventoIdAsync(eventoId);

        if (transmision == null)
        {
            return NotFound(new { Mensaje = "Transmisión no encontrada para el evento especificado." });
        }

        return Ok(new TransmisionDto(
            transmision.Id,
            transmision.EventoId,
            transmision.Plataforma,
            transmision.UrlAcceso,
            transmision.Estado
        ));
    }

    [HttpPost("{eventoId:guid}")]
    public async Task<ActionResult<TransmisionDto>> CrearObtenerTransmision(Guid eventoId)
    {
        // Verificar si ya existe una transmisión para este evento
        var transmisionExistente = await _repositorio.ObtenerPorEventoIdAsync(eventoId);
        
        if (transmisionExistente != null)
        {
            return Ok(new TransmisionDto(
                transmisionExistente.Id,
                transmisionExistente.EventoId,
                transmisionExistente.Plataforma,
                transmisionExistente.UrlAcceso,
                transmisionExistente.Estado
            ));
        }

        // Crear nueva transmisión con Google Meet por defecto
        var nuevaTransmision = Transmision.Crear(eventoId, PlataformaTransmision.GoogleMeet);
        
        await _repositorio.AgregarAsync(nuevaTransmision);
        await _unitOfWork.GuardarCambiosAsync();

        return CreatedAtAction(
            nameof(ObtenerPorEvento),
            new { eventoId = nuevaTransmision.EventoId },
            new TransmisionDto(
                nuevaTransmision.Id,
                nuevaTransmision.EventoId,
                nuevaTransmision.Plataforma,
                nuevaTransmision.UrlAcceso,
                nuevaTransmision.Estado
            )
        );
    }

    [HttpPut("{eventoId:guid}/url")]
    public async Task<ActionResult<TransmisionDto>> ActualizarUrl(Guid eventoId, [FromBody] ActualizarUrlRequest request)
    {
        var transmision = await _repositorio.ObtenerPorEventoIdAsync(eventoId);
        if (transmision == null) return NotFound();

        transmision.ActualizarUrl(request.Url);
        await _unitOfWork.GuardarCambiosAsync();

        return Ok(new TransmisionDto(
            transmision.Id,
            transmision.EventoId,
            transmision.Plataforma,
            transmision.UrlAcceso,
            transmision.Estado
        ));
    }
}

public record ActualizarUrlRequest(string Url);
