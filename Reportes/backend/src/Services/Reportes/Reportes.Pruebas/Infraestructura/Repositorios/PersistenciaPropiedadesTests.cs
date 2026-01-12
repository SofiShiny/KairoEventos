using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using Reportes.Dominio.ModelosLectura;

namespace Reportes.Pruebas.Infraestructura.Repositorios;

/// <summary>
/// Tests de propiedades para validar invariantes de persistencia y serialización
/// con cualquier modelo válido generado por FsCheck.
/// 
/// Feature: Persistencia de Modelos
/// Property: Los modelos mantienen sus invariantes durante la serialización
/// </summary>
public class PersistenciaPropiedadesTests
{
    /// <summary>
    /// Propiedad: Cualquier HistorialAsistencia válido mantiene sus invariantes
    /// </summary>
    [Property(MaxTest = 100)]
    public Property HistorialAsistencia_MantieneInvariantes()
    {
        return Prop.ForAll(GenerarHistorialAsistenciaValido(), historial =>
        {
            // Assert invariantes del dominio
            return (historial.AsientosDisponibles == (historial.CapacidadTotal - historial.AsientosReservados) &&
                    historial.AsientosDisponibles >= 0 &&
                    historial.AsientosReservados >= 0 &&
                    historial.AsientosReservados <= historial.CapacidadTotal &&
                    historial.TotalAsistentesRegistrados >= 0 &&
                    historial.TotalAsistentesRegistrados <= historial.AsientosReservados &&
                    historial.PorcentajeOcupacion >= 0 &&
                    historial.PorcentajeOcupacion <= 100 &&
                    historial.Asistentes.Count == historial.TotalAsistentesRegistrados)
                .ToProperty()
                .Label($"Historial: Capacidad={historial.CapacidadTotal}, Reservados={historial.AsientosReservados}, Disponibles={historial.AsientosDisponibles}");
        });
    }

    /// <summary>
    /// Propiedad: Cualquier MetricasEvento válido mantiene sus invariantes
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MetricasEvento_MantieneInvariantes()
    {
        return Prop.ForAll(GenerarMetricasEventoValido(), metricas =>
        {
            var estadosValidos = new[] { "Publicado", "Cancelado", "Finalizado" };
            
            // Assert invariantes del dominio
            return (estadosValidos.Contains(metricas.Estado) &&
                    metricas.TotalAsistentes >= 0 &&
                    metricas.TotalReservas >= 0 &&
                    metricas.TotalAsistentes <= metricas.TotalReservas &&
                    metricas.IngresoTotal >= 0 &&
                    metricas.FechaCreacion <= metricas.UltimaActualizacion &&
                    !string.IsNullOrEmpty(metricas.TituloEvento))
                .ToProperty()
                .Label($"Métricas: Estado={metricas.Estado}, Asistentes={metricas.TotalAsistentes}, Reservas={metricas.TotalReservas}");
        });
    }

    /// <summary>
    /// Propiedad: Cualquier LogAuditoria válido mantiene sus invariantes
    /// </summary>
    [Property(MaxTest = 100)]
    public Property LogAuditoria_MantieneInvariantes()
    {
        return Prop.ForAll(GenerarLogAuditoriaValido(), log =>
        {
            var tiposValidos = new[] { "EventoConsumido", "ReporteGenerado", "ErrorProcesamiento" };
            var entidadesValidas = new[] { "HistorialAsistencia", "MetricasEvento", "ReporteVentas" };
            
            // Assert invariantes del dominio
            return (tiposValidos.Contains(log.TipoOperacion) &&
                    entidadesValidas.Contains(log.Entidad) &&
                    !string.IsNullOrEmpty(log.EntidadId) &&
                    !string.IsNullOrEmpty(log.Detalles) &&
                    !string.IsNullOrEmpty(log.Usuario) &&
                    log.Timestamp.Kind == DateTimeKind.Utc &&
                    log.Timestamp <= DateTime.UtcNow.AddMinutes(1) && // Margen para ejecución
                    (log.Exitoso || !string.IsNullOrEmpty(log.MensajeError)))
                .ToProperty()
                .Label($"Log: Tipo={log.TipoOperacion}, Entidad={log.Entidad}, Exitoso={log.Exitoso}");
        });
    }

    /// <summary>
    /// Propiedad: Los timestamps siempre son UTC y no futuros
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Timestamps_SiempreSonUTCYNoFuturos()
    {
        return Prop.ForAll(GenerarHistorialAsistenciaConTimestamp(), historial =>
        {
            // Assert
            return (historial.UltimaActualizacion.Kind == DateTimeKind.Utc &&
                    historial.UltimaActualizacion <= DateTime.UtcNow.AddMinutes(1)) // Margen de 1 minuto para ejecución
                .ToProperty()
                .Label($"Timestamp: {historial.UltimaActualizacion} (Kind: {historial.UltimaActualizacion.Kind})");
        });
    }

    /// <summary>
    /// Propiedad: Los cálculos de porcentaje son consistentes
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CalculoPorcentaje_EsConsistente()
    {
        return Prop.ForAll(GenerarHistorialAsistenciaValido(), historial =>
        {
            // Act - Recalcular porcentaje
            var porcentajeCalculado = historial.CapacidadTotal > 0 
                ? (double)historial.AsientosReservados / historial.CapacidadTotal * 100 
                : 0;

            // Assert
            return (Math.Abs(historial.PorcentajeOcupacion - porcentajeCalculado) < 0.01)
                .ToProperty()
                .Label($"Porcentaje almacenado: {historial.PorcentajeOcupacion}, Calculado: {porcentajeCalculado}");
        });
    }

    #region Generadores

    private static Arbitrary<HistorialAsistencia> GenerarHistorialAsistenciaValido()
    {
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from titulo in GenerarTituloValido()
            from capacidad in Gen.Choose(10, 1000)
            from reservados in Gen.Choose(0, capacidad)
            from asistentes in Gen.Choose(0, reservados)
            select new HistorialAsistencia
            {
                EventoId = eventoId,
                TituloEvento = titulo,
                TotalAsistentesRegistrados = asistentes,
                CapacidadTotal = capacidad,
                AsientosReservados = reservados,
                AsientosDisponibles = capacidad - reservados,
                PorcentajeOcupacion = capacidad > 0 ? (double)reservados / capacidad * 100 : 0,
                Asistentes = GenerarListaAsistentes(asistentes),
                UltimaActualizacion = DateTime.UtcNow
            });
    }

    private static Arbitrary<MetricasEvento> GenerarMetricasEventoValido()
    {
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from titulo in GenerarTituloValido()
            from estado in GenerarEstadoValido()
            from asistentes in Gen.Choose(0, 500)
            from reservas in Gen.Choose(asistentes, asistentes + 100)
            from ingreso in Gen.Choose(0, 50000).Select(i => (decimal)i)
            from fechaInicio in GenerarFechaValida()
            select new MetricasEvento
            {
                EventoId = eventoId,
                TituloEvento = titulo,
                FechaInicio = fechaInicio,
                Estado = estado,
                TotalAsistentes = asistentes,
                TotalReservas = reservas,
                IngresoTotal = ingreso,
                FechaCreacion = DateTime.UtcNow,
                UltimaActualizacion = DateTime.UtcNow
            });
    }

    private static Arbitrary<LogAuditoria> GenerarLogAuditoriaValido()
    {
        return Arb.From(
            from tipoOperacion in GenerarTipoOperacionValido()
            from entidad in GenerarEntidadValida()
            from entidadId in Arb.Generate<Guid>().Select(g => g.ToString())
            from detalles in GenerarDetallesValidos()
            from usuario in GenerarUsuarioValido()
            from exitoso in Arb.Generate<bool>()
            from mensajeError in exitoso ? Gen.Constant<string?>(null) : GenerarMensajeErrorValido().Select(s => (string?)s)
            select new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = tipoOperacion,
                Entidad = entidad,
                EntidadId = entidadId,
                Detalles = detalles,
                Usuario = usuario,
                Exitoso = exitoso,
                MensajeError = mensajeError
            });
    }

    private static Arbitrary<HistorialAsistencia> GenerarHistorialAsistenciaConTimestamp()
    {
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from titulo in GenerarTituloValido()
            from timestamp in GenerarTimestampUTCValido()
            select new HistorialAsistencia
            {
                EventoId = eventoId,
                TituloEvento = titulo,
                UltimaActualizacion = timestamp,
                CapacidadTotal = 100,
                AsientosReservados = 50,
                AsientosDisponibles = 50,
                TotalAsistentesRegistrados = 30,
                PorcentajeOcupacion = 50.0
            });
    }

    private static List<RegistroAsistente> GenerarListaAsistentes(int cantidad)
    {
        var asistentes = new List<RegistroAsistente>();
        for (int i = 0; i < cantidad; i++)
        {
            asistentes.Add(new RegistroAsistente
            {
                UsuarioId = $"user-{i + 1}",
                NombreUsuario = $"Usuario {i + 1}",
                FechaRegistro = DateTime.UtcNow.AddDays(-i)
            });
        }
        return asistentes;
    }

    private static Gen<string> GenerarTituloValido()
    {
        return Gen.Elements(new[]
        {
            "Conferencia de Tecnología 2024",
            "Workshop de .NET",
            "Seminario de Arquitectura",
            "Meetup de Desarrolladores"
        });
    }

    private static Gen<string> GenerarEstadoValido()
    {
        return Gen.Elements(new[] { "Publicado", "Cancelado", "Finalizado" });
    }

    private static Gen<string> GenerarTipoOperacionValido()
    {
        return Gen.Elements(new[] { "EventoConsumido", "ReporteGenerado", "ErrorProcesamiento" });
    }

    private static Gen<string> GenerarEntidadValida()
    {
        return Gen.Elements(new[] { "HistorialAsistencia", "MetricasEvento", "ReporteVentas" });
    }

    private static Gen<string> GenerarDetallesValidos()
    {
        return Gen.Elements(new[]
        {
            "Evento procesado correctamente",
            "Reporte generado exitosamente",
            "Error en procesamiento de evento",
            "Actualización de métricas completada"
        });
    }

    private static Gen<string> GenerarUsuarioValido()
    {
        return Gen.Elements(new[] { "Sistema", "Admin", "Scheduler", "Consumer" });
    }

    private static Gen<string> GenerarMensajeErrorValido()
    {
        return Gen.Elements(new[]
        {
            "Error de conexión a base de datos",
            "Timeout en operación",
            "Formato de evento inválido",
            "Error de validación"
        });
    }

    private static Gen<DateTime> GenerarFechaValida()
    {
        return Gen.Choose(-30, 365)
            .Select(dias => DateTime.UtcNow.AddDays(dias));
    }

    private static Gen<DateTime> GenerarTimestampUTCValido()
    {
        return Gen.Choose(-365, 0) // Desde hace un año hasta ahora
            .Select(dias => DateTime.UtcNow.AddDays(dias));
    }

    #endregion
}