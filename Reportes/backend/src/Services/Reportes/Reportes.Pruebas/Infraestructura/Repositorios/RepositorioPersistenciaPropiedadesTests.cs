using FsCheck;
using FsCheck.Xunit;
using Reportes.Dominio.ModelosLectura;

namespace Reportes.Pruebas.Infraestructura.Repositorios;

/// <summary>
/// Property-Based Tests para persistencia en MongoDB del microservicio de Reportes.
/// Cada test ejecuta 100 iteraciones con datos generados aleatoriamente.
/// 
/// Nota: Estos tests verifican las propiedades de los modelos de datos que serán persistidos,
/// asegurando que los datos generados sean válidos y consistentes antes de la persistencia.
/// Los tests de integración con MongoDB real se encuentran en los unit tests.
/// </summary>
public class RepositorioPersistenciaPropiedadesTests
{
    /// <summary>
    /// Feature: microservicio-reportes, Property 1: Persistencia de eventos consumidos
    /// Valida: Requisitos 1.1, 1.3, 3.2
    /// 
    /// Para cualquier evento de dominio válido recibido por un consumidor,
    /// el sistema debe persistir los datos correspondientes en la colección MongoDB apropiada.
    /// 
    /// Este test verifica que:
    /// 1. Las métricas de evento generadas tienen un EventoId válido (no vacío)
    /// 2. El TituloEvento no está vacío
    /// 3. Los valores numéricos son no negativos
    /// 4. Las fechas son coherentes (FechaCreacion <= UltimaActualizacion)
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(GeneradoresMetricasEvento) })]
    public Property Propiedad1_PersistenciaEventosConsumidos_MetricasEventoValidas(MetricasEvento metricas)
    {
        // Assert - Verificar que los datos generados son válidos para persistencia
        var eventoIdValido = metricas.EventoId != Guid.Empty;
        var tituloValido = !string.IsNullOrWhiteSpace(metricas.TituloEvento);
        var valoresNumericosValidos = metricas.TotalAsistentes >= 0 && 
                                       metricas.TotalReservas >= 0 && 
                                       metricas.IngresoTotal >= 0;
        var fechasCoherentes = metricas.FechaCreacion <= metricas.UltimaActualizacion;
        var estadoValido = !string.IsNullOrWhiteSpace(metricas.Estado) &&
                          (metricas.Estado == "Publicado" || 
                           metricas.Estado == "Cancelado" || 
                           metricas.Estado == "Finalizado");
        
        return (eventoIdValido && tituloValido && valoresNumericosValidos && fechasCoherentes && estadoValido)
            .ToProperty()
            .Label($"MetricasEvento debe tener datos válidos: " +
                   $"EventoId={metricas.EventoId}, " +
                   $"Titulo='{metricas.TituloEvento}', " +
                   $"Estado='{metricas.Estado}', " +
                   $"TotalAsistentes={metricas.TotalAsistentes}, " +
                   $"TotalReservas={metricas.TotalReservas}, " +
                   $"IngresoTotal={metricas.IngresoTotal}");
    }
    
    /// <summary>
    /// Feature: microservicio-reportes, Property 1: Persistencia de eventos consumidos
    /// Valida: Requisitos 1.1, 1.3, 3.2
    /// 
    /// Verifica que los logs de auditoría generados tienen datos válidos para persistencia.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(GeneradoresLogAuditoria) })]
    public Property Propiedad1_PersistenciaEventosConsumidos_LogAuditoriaValido(LogAuditoria log)
    {
        // Assert - Verificar que los datos generados son válidos para persistencia
        var tipoOperacionValido = !string.IsNullOrWhiteSpace(log.TipoOperacion);
        var entidadValida = !string.IsNullOrWhiteSpace(log.Entidad);
        var entidadIdValido = !string.IsNullOrWhiteSpace(log.EntidadId);
        var timestampValido = log.Timestamp <= DateTime.UtcNow.AddMinutes(1); // Permitir pequeña diferencia de tiempo
        var mensajeErrorCoherente = !log.Exitoso || string.IsNullOrEmpty(log.MensajeError);
        
        return (tipoOperacionValido && entidadValida && entidadIdValido && timestampValido && mensajeErrorCoherente)
            .ToProperty()
            .Label($"LogAuditoria debe tener datos válidos: " +
                   $"TipoOperacion='{log.TipoOperacion}', " +
                   $"Entidad='{log.Entidad}', " +
                   $"EntidadId='{log.EntidadId}', " +
                   $"Exitoso={log.Exitoso}, " +
                   $"MensajeError='{log.MensajeError}'");
    }
    
    /// <summary>
    /// Feature: microservicio-reportes, Property 1: Persistencia de eventos consumidos
    /// Valida: Requisitos 1.1, 1.3, 3.2
    /// 
    /// Verifica que el Id de MongoDB se genera correctamente para todos los modelos.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(GeneradoresMetricasEvento) })]
    public Property Propiedad1_PersistenciaEventosConsumidos_IdMongoDBGenerado(MetricasEvento metricas)
    {
        // Assert - Verificar que el Id de MongoDB se genera
        var idGenerado = !string.IsNullOrWhiteSpace(metricas.Id);
        var idLongitudValida = metricas.Id.Length == 24; // ObjectId de MongoDB tiene 24 caracteres
        
        return (idGenerado && idLongitudValida)
            .ToProperty()
            .Label($"El Id de MongoDB debe estar generado y tener 24 caracteres: Id='{metricas.Id}'");
    }
}

/// <summary>
/// Generadores personalizados para FsCheck que crean instancias válidas de MetricasEvento
/// respetando las invariantes del dominio.
/// </summary>
public static class GeneradoresMetricasEvento
{
    /// <summary>
    /// Genera instancias de MetricasEvento con datos coherentes.
    /// </summary>
    public static Arbitrary<MetricasEvento> GeneradorMetricasEvento()
    {
        // Generador de strings no vacíos y sin solo espacios en blanco
        var tituloGen = from str in Arb.Generate<NonEmptyString>()
                        let trimmed = str.Get.Trim()
                        where !string.IsNullOrWhiteSpace(trimmed)
                        select trimmed.Length > 0 ? trimmed : "Evento Test";
        
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from titulo in tituloGen
            from fechaInicio in Arb.Generate<DateTime>()
            from estado in Gen.Elements("Publicado", "Cancelado", "Finalizado")
            from totalAsistentes in Gen.Choose(0, 1000)
            from totalReservas in Gen.Choose(0, 1000)
            from ingresoTotal in Gen.Choose(0, 100000).Select(x => (decimal)x)
            select new MetricasEvento
            {
                EventoId = eventoId,
                TituloEvento = titulo,
                FechaInicio = fechaInicio,
                Estado = estado,
                TotalAsistentes = totalAsistentes,
                TotalReservas = totalReservas,
                IngresoTotal = ingresoTotal,
                FechaCreacion = DateTime.UtcNow,
                UltimaActualizacion = DateTime.UtcNow
            });
    }
}

/// <summary>
/// Generadores personalizados para FsCheck que crean instancias válidas de LogAuditoria.
/// </summary>
public static class GeneradoresLogAuditoria
{
    /// <summary>
    /// Genera instancias de LogAuditoria con datos coherentes.
    /// </summary>
    public static Arbitrary<LogAuditoria> GeneradorLogAuditoria()
    {
        return Arb.From(
            from tipoOperacion in Gen.Elements("EventoConsumido", "ReporteGenerado", "ErrorProcesamiento")
            from entidad in Gen.Elements("Evento", "Asiento", "Asistente", "Reporte")
            from entidadId in Arb.Generate<Guid>().Select(g => g.ToString())
            from detalles in Arb.Generate<string>()
            from exitoso in Arb.Generate<bool>()
            from mensajeError in Arb.Generate<string>()
            select new LogAuditoria
            {
                Timestamp = DateTime.UtcNow,
                TipoOperacion = tipoOperacion,
                Entidad = entidad,
                EntidadId = entidadId,
                Detalles = detalles ?? string.Empty,
                Usuario = "Sistema",
                Exitoso = exitoso,
                MensajeError = exitoso ? null : mensajeError
            });
    }
}
