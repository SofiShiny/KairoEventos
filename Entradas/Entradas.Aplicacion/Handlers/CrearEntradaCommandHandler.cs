using System.Collections.Generic;
using System.Linq;
using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Entradas.Aplicacion.Comandos;
using Entradas.Aplicacion.DTOs;
using Entradas.Dominio.Entidades;
using Entradas.Dominio.Interfaces;
using Entradas.Dominio.Excepciones;
using Entradas.Dominio.Eventos;

namespace Entradas.Aplicacion.Handlers;

/// <summary>
/// Handler para procesar el comando CrearEntradaCommand con orquestación síncrona
/// </summary>
public class CrearEntradaCommandHandler : IRequestHandler<CrearEntradaCommand, EntradaCreadaDto>
{
    private readonly IVerificadorEventos _verificadorEventos;
    private readonly IVerificadorAsientos _verificadorAsientos;
    private readonly IGeneradorCodigoQr _generadorQr;
    private readonly IRepositorioEntradas _repositorio;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<CrearEntradaCommandHandler> _logger;
    private readonly IEntradasMetrics _metrics;
    private readonly ActivitySource _activitySource;
    private readonly IServicioDescuentos _servicioDescuentos;
    private readonly IAsientosService _asientosService;

    public CrearEntradaCommandHandler(
        IVerificadorEventos verificadorEventos,
        IVerificadorAsientos verificadorAsientos,
        IGeneradorCodigoQr generadorQr,
        IRepositorioEntradas repositorio,
        IPublishEndpoint publisher,
        ILogger<CrearEntradaCommandHandler> logger,
        IEntradasMetrics metrics,
        ActivitySource activitySource,
        IServicioDescuentos servicioDescuentos,
        IAsientosService asientosService)
    {
        _verificadorEventos = verificadorEventos ?? throw new ArgumentNullException(nameof(verificadorEventos));
        _verificadorAsientos = verificadorAsientos ?? throw new ArgumentNullException(nameof(verificadorAsientos));
        _generadorQr = generadorQr ?? throw new ArgumentNullException(nameof(generadorQr));
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
        _servicioDescuentos = servicioDescuentos ?? throw new ArgumentNullException(nameof(servicioDescuentos));
        _asientosService = asientosService ?? throw new ArgumentNullException(nameof(asientosService));
    }

    public async Task<EntradaCreadaDto> Handle(CrearEntradaCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        using var activity = _activitySource.StartActivity("CrearEntrada");
        activity?.SetTag("evento.id", request.EventoId.ToString());
        activity?.SetTag("usuario.id", request.UsuarioId.ToString());
        activity?.SetTag("asiento.id", request.AsientoId?.ToString() ?? "null");

        _logger.LogInformation("Iniciando creación de entrada para evento {EventoId} y usuario {UsuarioId}", 
            request.EventoId, request.UsuarioId);

        try
        {
            // 1. Validar que el evento existe y está disponible (orquestación síncrona)
            _logger.LogDebug("Verificando disponibilidad del evento {EventoId}", request.EventoId);
            
            var eventoStopwatch = Stopwatch.StartNew();
            var eventoDisponible = await _verificadorEventos.EventoExisteYDisponibleAsync(request.EventoId, cancellationToken);
            eventoStopwatch.Stop();
            
            _metrics.RecordServicioExternoDuration("eventos", eventoStopwatch.Elapsed.TotalMilliseconds, 
                eventoDisponible ? "success" : "not_found");
            
            if (!eventoDisponible)
            {
                _logger.LogWarning("Evento {EventoId} no existe o no está disponible", request.EventoId);
                _metrics.IncrementValidacionExternaError("eventos", "evento_no_disponible");
                activity?.SetStatus(ActivityStatusCode.Error, "Evento no disponible");
                throw new EventoNoDisponibleException(request.EventoId, $"El evento {request.EventoId} no existe o no está disponible para la venta de entradas");
            }

            // 2. Validar que el asiento está disponible (si aplica)
            if (request.AsientoId.HasValue)
            {
                _logger.LogDebug("Verificando disponibilidad del asiento {AsientoId} para evento {EventoId}", 
                    request.AsientoId, request.EventoId);
                
                var asientoStopwatch = Stopwatch.StartNew();
                var asientoDisponible = await _verificadorAsientos.AsientoDisponibleAsync(
                    request.EventoId, request.AsientoId, cancellationToken);
                asientoStopwatch.Stop();
                
                _metrics.RecordServicioExternoDuration("asientos", asientoStopwatch.Elapsed.TotalMilliseconds,
                    asientoDisponible ? "success" : "not_available");
                
                if (!asientoDisponible)
                {
                    _logger.LogWarning("Asiento {AsientoId} no está disponible para evento {EventoId}", 
                        request.AsientoId, request.EventoId);
                    _metrics.IncrementValidacionExternaError("asientos", "asiento_no_disponible");
                    activity?.SetStatus(ActivityStatusCode.Error, "Asiento no disponible");
                    throw new AsientoNoDisponibleException(request.EventoId, request.AsientoId, $"El asiento {request.AsientoId} no está disponible para el evento {request.EventoId}");
                }

                // Reservar el asiento
                _logger.LogInformation("Reservando asiento {AsientoId} para el usuario {UsuarioId}", request.AsientoId, request.UsuarioId);
                await _verificadorAsientos.ReservarAsientoTemporalAsync(request.EventoId, request.AsientoId.Value, request.UsuarioId, TimeSpan.FromMinutes(10), cancellationToken);
            }

            // 3. Generar código QR único
            var codigoQr = _generadorQr.GenerarCodigoUnico();
            _logger.LogDebug("Código QR generado: {CodigoQr}", codigoQr);
            activity?.SetTag("codigo.qr", codigoQr);

            // 3. Obtener Precio y Datos descriptivos (Snapshot)
            decimal montoAsiento = 0;
            string? tituloEvento = request.TituloEvento;
            string? imagenEventoUrl = request.ImagenEventoUrl;
            DateTime? fechaEvento = request.FechaEvento;
            string? nombreSector = request.NombreSector;
            string? fila = request.Fila;
            int? numeroAsiento = request.NumeroAsiento;

            // Obtener info del evento (Título y Fecha siempre necesarios)
            var infoEvento = await _verificadorEventos.ObtenerInfoEventoAsync(request.EventoId, cancellationToken);
            if (infoEvento != null)
            {
                tituloEvento ??= infoEvento.Nombre;
                imagenEventoUrl ??= infoEvento.UrlImagen;
                fechaEvento ??= infoEvento.FechaEvento;
            }

            if (request.AsientoId.HasValue)
            {
                var infoAsiento = await _asientosService.GetAsientoByIdAsync(request.AsientoId.Value, cancellationToken);
                if (infoAsiento != null)
                {
                    montoAsiento = infoAsiento.Precio;
                    nombreSector ??= infoAsiento.Seccion;
                    fila ??= infoAsiento.Fila.ToString();
                    numeroAsiento ??= infoAsiento.Numero;
                }
            }
            else
            {
                montoAsiento = infoEvento?.PrecioBase ?? 0;
            }

            // 4. Calcular Descuentos
            decimal montoDescuento = 0;
            string? cuponesJson = null;

            List<string>? cuponesRequest = request.Cupones;
            if (cuponesRequest != null && cuponesRequest.Any())
            {
                var resultadoDescuento = await _servicioDescuentos.CalcularDescuentoAsync(
                    request.UsuarioId, cuponesRequest, montoAsiento);
                
                if (resultadoDescuento.Aplicado)
                {
                    montoDescuento = resultadoDescuento.MontoDescuento;
                    cuponesJson = System.Text.Json.JsonSerializer.Serialize(request.Cupones);
                    _logger.LogInformation("Descuento de {Monto} aplicado para usuario {UsuarioId}", montoDescuento, request.UsuarioId);
                }
            }

            // 5. Crear entidad Entrada usa factory method del dominio (Actualizado con montos y snapshot)
            var entrada = Entrada.Crear(
                request.EventoId,
                request.UsuarioId,
                montoAsiento, // Precio obtenido del servicio seguro
                request.AsientoId,
                codigoQr,
                montoDescuento,
                cuponesJson,
                tituloEvento,
                imagenEventoUrl,
                request.Categoria,
                fechaEvento,
                nombreSector,
                fila,
                numeroAsiento);

            _logger.LogDebug("Entidad Entrada creada con ID {EntradaId}", entrada.Id);
            activity?.SetTag("entrada.id", entrada.Id.ToString());

            // 5. Persistir en base de datos
            var entradaGuardada = await _repositorio.GuardarAsync(entrada, cancellationToken);
            _logger.LogInformation("Entrada {EntradaId} persistida exitosamente", entradaGuardada.Id);

            // 6. Publicar evento EntradaCreadaEvento
            var evento = new EntradaCreadaEvento
            {
                EntradaId = entradaGuardada.Id,
                EventoId = entradaGuardada.EventoId,
                UsuarioId = entradaGuardada.UsuarioId,
                Monto = entradaGuardada.Monto,
                MontoDescuento = entradaGuardada.MontoDescuento,
                CuponesAplicados = entradaGuardada.CuponesAplicados,
                FechaCreacion = entradaGuardada.FechaCompra,
                CodigoQr = entradaGuardada.CodigoQr,
                NombreUsuario = request.NombreUsuario,
                Email = request.Email
            };

            await _publisher.Publish(evento, cancellationToken);
            _logger.LogInformation("Evento EntradaCreadaEvento publicado para entrada {EntradaId}", entradaGuardada.Id);

            // 7. Registrar métricas de éxito
            stopwatch.Stop();
            _metrics.IncrementEntradasCreadas(request.EventoId.ToString(), entradaGuardada.Estado.ToString());
            _metrics.RecordCreacionDuration(stopwatch.Elapsed.TotalMilliseconds, "success");

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("resultado", "success");

            // 8. Retornar DTO de respuesta
            return new EntradaCreadaDto(
                entradaGuardada.Id,
                entradaGuardada.EventoId,
                entradaGuardada.UsuarioId,
                entradaGuardada.AsientoId,
                entradaGuardada.Monto,
                entradaGuardada.CodigoQr,
                entradaGuardada.Estado,
                entradaGuardada.FechaCompra);
        }
        catch (EventoNoDisponibleException)
        {
            stopwatch.Stop();
            _logger.LogError("Fallo en validación: evento no disponible");
            _metrics.RecordCreacionDuration(stopwatch.Elapsed.TotalMilliseconds, "evento_no_disponible");
            throw;
        }
        catch (AsientoNoDisponibleException)
        {
            stopwatch.Stop();
            _logger.LogError("Fallo en validación: asiento no disponible");
            _metrics.RecordCreacionDuration(stopwatch.Elapsed.TotalMilliseconds, "asiento_no_disponible");
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error inesperado al crear entrada para evento {EventoId}", request.EventoId);
            _metrics.RecordCreacionDuration(stopwatch.Elapsed.TotalMilliseconds, "error");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}