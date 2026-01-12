using System;
using Microsoft.AspNetCore.Mvc;
using Streaming.Aplicacion.DTOs;
using Streaming.Dominio.Interfaces;

namespace Streaming.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StreamingController : ControllerBase
{
    private readonly IRepositorioTransmisiones _repositorio;

    public StreamingController(IRepositorioTransmisiones repositorio)
    {
        _repositorio = repositorio;
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
}
