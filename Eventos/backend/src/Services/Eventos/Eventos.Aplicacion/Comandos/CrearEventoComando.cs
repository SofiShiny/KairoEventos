using System;
using BloquesConstruccion.Aplicacion.Comandos;
using BloquesConstruccion.Aplicacion.Comun;
using Eventos.Aplicacion.DTOs;

namespace Eventos.Aplicacion.Comandos;

public record CrearEventoComando(
    string Titulo,
    string Descripcion,
    UbicacionDto Ubicacion,
    DateTime FechaInicio,
    DateTime FechaFin,
    int MaximoAsistentes,
    string OrganizadorId,
    string? Categoria = null,
    decimal PrecioBase = 0,
    bool EsVirtual = false
) : IComando<Resultado<EventoDto>>;
