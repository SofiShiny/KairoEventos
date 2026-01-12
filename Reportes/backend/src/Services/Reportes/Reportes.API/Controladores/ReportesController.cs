using Microsoft.AspNetCore.Mvc;
using Reportes.API.DTOs;
using Reportes.Dominio.Repositorios;

namespace Reportes.API.Controladores;

[ApiController]
[Route("api/reportes")]
[Produces("application/json")]
public class ReportesController : ControllerBase
{
    private readonly IRepositorioReportesLectura _repositorio;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(
        IRepositorioReportesLectura repositorio,
        ILogger<ReportesController> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene un resumen de ventas para un rango de fechas
    /// </summary>
    /// <remarks>
    /// Retorna métricas agregadas de ventas incluyendo total de ingresos, cantidad de reservas y promedio por evento.
    /// Si no se especifican fechas, retorna datos de los últimos 30 días.
    /// 
    /// Ejemplo de request:
    /// 
    ///     GET /api/reportes/resumen-ventas?fechaInicio=2024-01-01&amp;fechaFin=2024-01-31
    /// 
    /// Ejemplo de respuesta exitosa:
    /// 
    ///     {
    ///       "fechaInicio": "2024-01-01T00:00:00Z",
    ///       "fechaFin": "2024-01-31T00:00:00Z",
    ///       "totalVentas": 15000.00,
    ///       "cantidadReservas": 150,
    ///       "promedioEvento": 30.5,
    ///       "ventasPorEvento": [
    ///         {
    ///           "eventoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///           "tituloEvento": "Concierto Rock 2024",
    ///           "cantidadReservas": 75,
    ///           "totalIngresos": 7500.00
    ///         }
    ///       ]
    ///     }
    /// </remarks>
    /// <param name="fechaInicio">Fecha de inicio del período (opcional, por defecto: hace 30 días)</param>
    /// <param name="fechaFin">Fecha de fin del período (opcional, por defecto: hoy)</param>
    /// <response code="200">Resumen de ventas obtenido exitosamente</response>
    /// <response code="400">Parámetros inválidos (ej: fechaInicio mayor que fechaFin)</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("resumen-ventas")]
    [ProducesResponseType(typeof(ResumenVentasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResumenVentasDto>> ObtenerResumenVentas(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        try
        {
            // Validar parámetros
            if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
            {
                return BadRequest(new { error = "fechaInicio debe ser menor o igual a fechaFin" });
            }

            // Valores por defecto: últimos 30 días
            var inicio = fechaInicio ?? DateTime.UtcNow.AddDays(-30).Date;
            var fin = fechaFin ?? DateTime.UtcNow.Date;

            _logger.LogInformation(
                "Obteniendo resumen de ventas: {FechaInicio} - {FechaFin}",
                inicio, fin);

            var ventas = await _repositorio.ObtenerVentasPorRangoAsync(inicio, fin);

            var resumen = new ResumenVentasDto
            {
                FechaInicio = inicio,
                FechaFin = fin,
                TotalVentas = ventas.Sum(v => v.TotalIngresos),
                CantidadReservas = ventas.Sum(v => v.CantidadReservas),
                PromedioEvento = ventas.Any() 
                    ? ventas.Average(v => v.CantidadReservas) 
                    : 0,
                VentasPorEvento = ventas
                    .GroupBy(v => v.EventoId)
                    .Select(g => new VentaPorEventoDto
                    {
                        EventoId = g.Key,
                        TituloEvento = g.First().TituloEvento,
                        CantidadReservas = g.Sum(v => v.CantidadReservas),
                        TotalIngresos = g.Sum(v => v.TotalIngresos)
                    })
                    .ToList()
            };

            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo resumen de ventas");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene métricas completas de un evento con análisis de descuentos y cupones
    /// </summary>
    /// <remarks>
    /// Retorna métricas detalladas del evento incluyendo:
    /// - **Ingresos totales** y **descuentos aplicados**
    /// - Cantidad de **asistentes** y **reservas**
    /// - **Top 5 cupones** más utilizados ordenados por frecuencia de uso
    /// - Estado actual del evento
    /// - Fecha de publicación y última actualización
    /// 
    /// Este endpoint es ideal para dashboards financieros y análisis de campañas promocionales.
    /// 
    /// **Ejemplo de request:**
    /// 
    ///     GET /api/reportes/metricas-evento/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// 
    /// **Ejemplo de respuesta exitosa:**
    /// 
    ///     {
    ///       "eventoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "tituloEvento": "Concierto Rock 2024",
    ///       "totalAsistentes": 150,
    ///       "totalReservas": 200,
    ///       "ingresoTotal": 15000.00,
    ///       "totalDescuentos": 2500.00,
    ///       "topCupones": [
    ///         {
    ///           "codigo": "VERANO2024",
    ///           "cantidad": 45
    ///         },
    ///         {
    ///           "codigo": "ESTUDIANTE",
    ///           "cantidad": 30
    ///         }
    ///       ],
    ///       "estado": "Publicado",
    ///       "fechaPublicacion": "2024-01-01T10:00:00Z",
    ///       "ultimaActualizacion": "2024-06-15T18:30:00Z"
    ///     }
    /// 
    /// **Casos de uso:**
    /// - Análisis de efectividad de campañas de descuento
    /// - Cálculo de ROI de cupones promocionales
    /// - Reportes financieros para stakeholders
    /// - Dashboards de métricas en tiempo real
    /// </remarks>
    /// <param name="eventoId">ID único del evento (GUID)</param>
    /// <response code="200">Métricas del evento obtenidas exitosamente</response>
    /// <response code="404">Evento no encontrado o sin métricas disponibles</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("metricas-evento/{eventoId}")]
    [ProducesResponseType(typeof(MetricasEventoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MetricasEventoDto>> ObtenerMetricasEvento(Guid eventoId)
    {
        try
        {
            _logger.LogInformation("Obteniendo métricas para evento {EventoId}", eventoId);

            var metricas = await _repositorio.ObtenerMetricasEventoAsync(eventoId);

            if (metricas == null)
            {
                return NotFound(new { mensaje = $"No se encontraron métricas para el evento {eventoId}" });
            }

            // Mapear a DTO incluyendo descuentos y cupones (Top 5)
            var dto = new API.DTOs.MetricasEventoDto
            {
                EventoId = metricas.EventoId,
                TituloEvento = metricas.TituloEvento,
                TotalAsistentes = metricas.TotalAsistentes,
                TotalReservas = metricas.TotalReservas,
                IngresoTotal = metricas.IngresoTotal,
                TotalDescuentos = metricas.TotalDescuentos,
                TopCupones = metricas.UsoDeCupones
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(5)
                    .Select(kvp => new Aplicacion.DTOs.CuponUsoDto
                    {
                        Codigo = kvp.Key,
                        Cantidad = kvp.Value
                    })
                    .ToList(),
                Estado = metricas.Estado,
                FechaPublicacion = metricas.FechaCreacion,
                UltimaActualizacion = metricas.UltimaActualizacion
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo métricas para evento {EventoId}", eventoId);
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene logs de auditoría filtrados por evento
    /// </summary>
    /// <param name="eventoId">ID del evento para filtrar logs</param>
    /// <response code="200">Logs obtenidos exitosamente</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("logs-auditoria")]
    [ProducesResponseType(typeof(List<LogAuditoriaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<LogAuditoriaDto>>> ObtenerLogsAuditoriaPorEvento([FromQuery] Guid? eventoId)
    {
        try
        {
            _logger.LogInformation("Obteniendo logs de auditoría para evento {EventoId}", eventoId);

            var logs = await _repositorio.ObtenerLogsAuditoriaAsync(null, null, null, 1, 100);
            
            if (eventoId.HasValue)
            {
                logs = logs.Where(l => l.EntidadId == eventoId.Value.ToString()).ToList();
            }

            var resultado = logs.Select(l => new LogAuditoriaDto
            {
                Id = l.Id,
                Timestamp = l.Timestamp,
                TipoOperacion = l.TipoOperacion,
                Entidad = l.Entidad,
                EntidadId = l.EntidadId,
                Detalles = l.Detalles,
                Exitoso = l.Exitoso,
                MensajeError = l.MensajeError
            }).ToList();

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo logs de auditoría");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene la asistencia y aforo de un evento específico
    /// </summary>
    /// <remarks>
    /// Retorna información detallada sobre la asistencia de un evento, incluyendo:
    /// - Total de asistentes registrados
    /// - Asientos reservados y disponibles
    /// - Porcentaje de ocupación
    /// - Última actualización de datos
    /// 
    /// Ejemplo de request:
    /// 
    ///     GET /api/reportes/asistencia/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// 
    /// Ejemplo de respuesta exitosa:
    /// 
    ///     {
    ///       "eventoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "tituloEvento": "Concierto Rock 2024",
    ///       "totalAsistentes": 150,
    ///       "asientosReservados": 120,
    ///       "asientosDisponibles": 80,
    ///       "capacidadTotal": 200,
    ///       "porcentajeOcupacion": 60.0,
    ///       "ultimaActualizacion": "2024-01-15T10:30:00Z"
    ///     }
    /// </remarks>
    /// <param name="eventoId">ID único del evento</param>
    /// <response code="200">Información de asistencia obtenida exitosamente</response>
    /// <response code="404">Evento no encontrado o sin información de asistencia</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("asistencia/{eventoId}")]
    [ProducesResponseType(typeof(AsistenciaEventoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AsistenciaEventoDto>> ObtenerAsistenciaEvento(Guid eventoId)
    {
        try
        {
            _logger.LogInformation("Obteniendo asistencia para evento {EventoId}", eventoId);

            var historial = await _repositorio.ObtenerAsistenciaEventoAsync(eventoId);

            if (historial == null)
            {
                return NotFound(new { mensaje = $"No se encontró información de asistencia para el evento {eventoId}" });
            }

            var dto = new AsistenciaEventoDto
            {
                EventoId = historial.EventoId,
                TituloEvento = historial.TituloEvento,
                TotalAsistentes = historial.TotalAsistentesRegistrados,
                AsientosReservados = historial.AsientosReservados,
                AsientosDisponibles = historial.AsientosDisponibles,
                CapacidadTotal = historial.CapacidadTotal,
                PorcentajeOcupacion = historial.PorcentajeOcupacion,
                UltimaActualizacion = historial.UltimaActualizacion
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo asistencia para evento {EventoId}", eventoId);
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene logs de auditoría con filtros y paginación
    /// </summary>
    /// <remarks>
    /// Retorna logs de auditoría del sistema con soporte para:
    /// - Filtrado por rango de fechas
    /// - Filtrado por tipo de operación
    /// - Paginación de resultados
    /// - Ordenamiento descendente por timestamp (más recientes primero)
    /// 
    /// Tipos de operación válidos:
    /// - EventoConsumido
    /// - ReporteGenerado
    /// - ErrorProcesamiento
    /// - ConsolidacionEjecutada
    /// 
    /// Ejemplo de request:
    /// 
    ///     GET /api/reportes/auditoria?tipoOperacion=EventoConsumido&amp;pagina=1&amp;tamañoPagina=20
    /// 
    /// Ejemplo de respuesta exitosa:
    /// 
    ///     {
    ///       "datos": [
    ///         {
    ///           "id": "507f1f77bcf86cd799439011",
    ///           "timestamp": "2024-01-15T10:30:00Z",
    ///           "tipoOperacion": "EventoConsumido",
    ///           "entidad": "Evento",
    ///           "entidadId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///           "detalles": "Evento publicado procesado correctamente",
    ///           "exitoso": true,
    ///           "mensajeError": null
    ///         }
    ///       ],
    ///       "paginaActual": 1,
    ///       "tamañoPagina": 20,
    ///       "totalRegistros": 150
    ///     }
    /// </remarks>
    /// <param name="fechaInicio">Fecha de inicio del filtro (opcional)</param>
    /// <param name="fechaFin">Fecha de fin del filtro (opcional)</param>
    /// <param name="tipoOperacion">Tipo de operación a filtrar (opcional)</param>
    /// <param name="pagina">Número de página (por defecto: 1, mínimo: 1)</param>
    /// <param name="tamañoPagina">Cantidad de registros por página (por defecto: 50, rango: 1-100)</param>
    /// <response code="200">Logs de auditoría obtenidos exitosamente</response>
    /// <response code="400">Parámetros inválidos (ej: página menor a 1, tamaño fuera de rango)</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("auditoria")]
    [ProducesResponseType(typeof(PaginacionDto<LogAuditoriaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginacionDto<LogAuditoriaDto>>> ObtenerAuditoria(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] string? tipoOperacion,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamañoPagina = 50)
    {
        try
        {
            // Validar parámetros
            if (pagina < 1)
            {
                return BadRequest(new { error = "pagina debe ser mayor o igual a 1" });
            }

            if (tamañoPagina < 1 || tamañoPagina > 100)
            {
                return BadRequest(new { error = "tamañoPagina debe estar entre 1 y 100" });
            }

            if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
            {
                return BadRequest(new { error = "fechaInicio debe ser menor o igual a fechaFin" });
            }

            _logger.LogInformation(
                "Obteniendo logs de auditoría: Página {Pagina}, Tamaño {Tamaño}",
                pagina, tamañoPagina);

            var logs = await _repositorio.ObtenerLogsAuditoriaAsync(
                fechaInicio, fechaFin, tipoOperacion, pagina, tamañoPagina);

            var totalRegistros = await _repositorio.ContarLogsAuditoriaAsync(
                fechaInicio, fechaFin, tipoOperacion);

            var resultado = new PaginacionDto<LogAuditoriaDto>
            {
                Datos = logs.Select(l => new LogAuditoriaDto
                {
                    Id = l.Id,
                    Timestamp = l.Timestamp,
                    TipoOperacion = l.TipoOperacion,
                    Entidad = l.Entidad,
                    EntidadId = l.EntidadId,
                    Detalles = l.Detalles,
                    Exitoso = l.Exitoso,
                    MensajeError = l.MensajeError
                }).ToList(),
                PaginaActual = pagina,
                TamañoPagina = tamañoPagina,
                TotalRegistros = totalRegistros
            };

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo logs de auditoría");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene datos de conciliación financiera
    /// </summary>
    /// <remarks>
    /// Retorna información consolidada de transacciones financieras para conciliación contable:
    /// - Total de ingresos en el período
    /// - Cantidad de transacciones procesadas
    /// - Desglose por categoría de asiento
    /// - Listado detallado de transacciones
    /// 
    /// Si no se especifican fechas, retorna datos del mes actual.
    /// 
    /// Ejemplo de request:
    /// 
    ///     GET /api/reportes/conciliacion-financiera?fechaInicio=2024-01-01&amp;fechaFin=2024-01-31
    /// 
    /// Ejemplo de respuesta exitosa:
    /// 
    ///     {
    ///       "fechaInicio": "2024-01-01T00:00:00Z",
    ///       "fechaFin": "2024-01-31T00:00:00Z",
    ///       "totalIngresos": 25000.00,
    ///       "cantidadTransacciones": 250,
    ///       "desglosePorCategoria": {
    ///         "VIP": 10000.00,
    ///         "General": 12000.00,
    ///         "Estudiante": 3000.00
    ///       },
    ///       "transacciones": [
    ///         {
    ///           "eventoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///           "tituloEvento": "Concierto Rock 2024",
    ///           "fecha": "2024-01-15T00:00:00Z",
    ///           "cantidadReservas": 50,
    ///           "monto": 5000.00
    ///         }
    ///       ]
    ///     }
    /// </remarks>
    /// <param name="fechaInicio">Fecha de inicio del período (opcional, por defecto: inicio del mes actual)</param>
    /// <param name="fechaFin">Fecha de fin del período (opcional, por defecto: hoy)</param>
    /// <response code="200">Datos de conciliación obtenidos exitosamente</response>
    /// <response code="400">Parámetros inválidos (ej: fechaInicio mayor que fechaFin)</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("conciliacion-financiera")]
    [ProducesResponseType(typeof(ConciliacionFinancieraDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ConciliacionFinancieraDto>> ObtenerConciliacionFinanciera(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        try
        {
            // Validar parámetros
            if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
            {
                return BadRequest(new { error = "fechaInicio debe ser menor o igual a fechaFin" });
            }

            // Valores por defecto: mes actual
            var inicio = fechaInicio ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var fin = fechaFin ?? DateTime.UtcNow.Date;

            _logger.LogInformation(
                "Obteniendo conciliación financiera: {FechaInicio} - {FechaFin}",
                inicio, fin);

            var ventas = await _repositorio.ObtenerVentasPorRangoAsync(inicio, fin);

            var conciliacion = new ConciliacionFinancieraDto
            {
                FechaInicio = inicio,
                FechaFin = fin,
                TotalIngresos = ventas.Sum(v => v.TotalIngresos),
                CantidadTransacciones = ventas.Sum(v => v.CantidadReservas),
                DesglosePorCategoria = ventas
                    .SelectMany(v => v.ReservasPorCategoria)
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(
                        g => g.Key,
                        g => (decimal)g.Sum(kvp => kvp.Value)
                    ),
                Transacciones = ventas.Select(v => new TransaccionDto
                {
                    EventoId = v.EventoId,
                    TituloEvento = v.TituloEvento,
                    Fecha = v.Fecha,
                    CantidadReservas = v.CantidadReservas,
                    Monto = v.TotalIngresos
                }).ToList()
            };

            return Ok(conciliacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo conciliación financiera");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult> ObtenerDashboardMetrics()
    {
        try
        {
            var hoy = DateTime.UtcNow.Date;
            var hace7Dias = hoy.AddDays(-6);

            var metricasSemana = await _repositorio.ObtenerMetricasDiariasPorRangoAsync(hace7Dias, hoy);
            var todasLasAsistencias = await _repositorio.ObtenerTodoHistorialAsistenciaAsync();

            var resumen = new
            {
                ventasSemana = metricasSemana.Select(m => new {
                    fecha = m.Fecha.ToString("yyyy-MM-dd"),
                    totalVentas = m.TotalVentas,
                    entradasVendidas = m.EntradasVendidas
                }),
                acumulado = new {
                    totalVentas = metricasSemana.Sum(m => (decimal)m.TotalVentas),
                    totalEntradas = metricasSemana.Sum(m => m.EntradasVendidas)
                },
                ocupacion = todasLasAsistencias.Select(a => new {
                    nombre = a.TituloEvento,
                    vendidas = a.AsientosReservados,
                    disponibles = a.AsientosDisponibles
                })
            };

            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo métricas del dashboard");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }
}
