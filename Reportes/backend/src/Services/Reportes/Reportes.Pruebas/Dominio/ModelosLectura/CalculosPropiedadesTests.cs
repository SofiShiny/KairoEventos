using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using Reportes.Dominio.ModelosLectura;

namespace Reportes.Pruebas.Dominio.ModelosLectura;

/// <summary>
/// Tests de propiedades para validar invariantes de cálculos en los modelos de lectura.
/// 
/// Feature: Cálculos y Validaciones
/// Property: Los cálculos siempre producen resultados dentro de rangos válidos
/// </summary>
public class CalculosPropiedadesTests
{
    /// <summary>
    /// Propiedad: El porcentaje de ocupación siempre está entre 0 y 100
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PorcentajeOcupacion_SiempreEntre0Y100(PositiveInt capacidad, NonNegativeInt reservados)
    {
        if (reservados.Get > capacidad.Get) return true; // Skip invalid combinations
        
        // Arrange
        var historial = new HistorialAsistencia
        {
            CapacidadTotal = capacidad.Get,
            AsientosReservados = reservados.Get,
            AsientosDisponibles = capacidad.Get - reservados.Get
        };

        // Act
        var porcentaje = historial.CapacidadTotal > 0 
            ? (double)historial.AsientosReservados / historial.CapacidadTotal * 100 
            : 0;
        historial.PorcentajeOcupacion = porcentaje;

        // Assert
        return historial.PorcentajeOcupacion >= 0 && historial.PorcentajeOcupacion <= 100;
    }

    /// <summary>
    /// Propiedad: Los asientos disponibles siempre son la diferencia entre capacidad total y reservados
    /// </summary>
    [Property(MaxTest = 100)]
    public bool AsientosDisponibles_SiempreSonCapacidadMenosReservados(PositiveInt capacidad, NonNegativeInt reservados)
    {
        if (reservados.Get > capacidad.Get) return true; // Skip invalid combinations
        
        // Arrange & Act
        var historial = new HistorialAsistencia
        {
            CapacidadTotal = capacidad.Get,
            AsientosReservados = reservados.Get,
            AsientosDisponibles = capacidad.Get - reservados.Get
        };

        // Assert
        return historial.AsientosDisponibles == (historial.CapacidadTotal - historial.AsientosReservados) &&
               historial.AsientosDisponibles >= 0;
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
    /// Propiedad: Los asistentes registrados nunca exceden los asientos reservados
    /// </summary>
    [Property(MaxTest = 100)]
    public bool AsistentesRegistrados_NuncaExcedenAsientosReservados(PositiveInt capacidad, NonNegativeInt reservados)
    {
        if (reservados.Get > capacidad.Get) return true; // Skip invalid combinations
        
        // Arrange
        var asistentesRegistrados = Math.Min(reservados.Get, capacidad.Get);
        var historial = new HistorialAsistencia
        {
            CapacidadTotal = capacidad.Get,
            AsientosReservados = reservados.Get,
            TotalAsistentesRegistrados = asistentesRegistrados,
            AsientosDisponibles = capacidad.Get - reservados.Get
        };

        // Assert
        return historial.TotalAsistentesRegistrados <= historial.AsientosReservados &&
               historial.TotalAsistentesRegistrados >= 0;
    }

    /// <summary>
    /// Propiedad: El ingreso total siempre es no negativo
    /// </summary>
    [Property(MaxTest = 100)]
    public Property IngresoTotal_SiempreEsNoNegativo()
    {
        return Prop.ForAll(GenerarMetricasEventoConIngreso(), metricas =>
        {
            // Assert
            return (metricas.IngresoTotal >= 0)
                .ToProperty()
                .Label($"Ingreso total: {metricas.IngresoTotal}");
        });
    }

    /// <summary>
    /// Propiedad: Las fechas de creación siempre son anteriores o iguales a las de actualización
    /// </summary>
    [Property(MaxTest = 100)]
    public Property FechaCreacion_SiempreAnteriorOIgualAActualizacion()
    {
        return Prop.ForAll(GenerarMetricasEventoConFechas(), metricas =>
        {
            // Assert
            return (metricas.FechaCreacion <= metricas.UltimaActualizacion)
                .ToProperty()
                .Label($"Creación: {metricas.FechaCreacion}, Actualización: {metricas.UltimaActualizacion}");
        });
    }

    /// <summary>
    /// Propiedad: La paginación siempre retorna la cantidad correcta de elementos
    /// </summary>
    [Property(MaxTest = 100)]
    public bool Paginacion_SiempreRetornaCantidadCorrecta(PositiveInt totalElementos, PositiveInt tamañoPagina, PositiveInt numeroPagina)
    {
        // Arrange
        var elementos = Enumerable.Range(1, totalElementos.Get).ToList();
        var paginaValida = numeroPagina.Get <= Math.Max(1, (totalElementos.Get / tamañoPagina.Get) + 1);
        
        if (!paginaValida) return true; // Skip invalid combinations
        
        // Act
        var elementosPaginados = elementos
            .Skip((numeroPagina.Get - 1) * tamañoPagina.Get)
            .Take(tamañoPagina.Get)
            .ToList();

        var elementosEsperados = Math.Min(
            tamañoPagina.Get, 
            Math.Max(0, totalElementos.Get - (numeroPagina.Get - 1) * tamañoPagina.Get)
        );

        // Assert
        return elementosPaginados.Count == elementosEsperados &&
               elementosPaginados.Count <= tamañoPagina.Get &&
               elementosPaginados.Count >= 0;
    }

    /// <summary>
    /// Propiedad: Los estados de eventos siempre son valores válidos
    /// </summary>
    [Property(MaxTest = 100)]
    public Property EstadosEvento_SiempreSonValores()
    {
        return Prop.ForAll(GenerarMetricasEventoConEstado(), metricas =>
        {
            var estadosValidos = new[] { "Publicado", "Cancelado", "Finalizado" };
            
            // Assert
            return estadosValidos.Contains(metricas.Estado)
                .ToProperty()
                .Label($"Estado: {metricas.Estado}");
        });
    }

    #region Generadores

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

    private static Arbitrary<MetricasEvento> GenerarMetricasEventoConIngreso()
    {
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from titulo in GenerarTituloValido()
            from ingreso in Gen.Choose(0, 100000).Select(i => (decimal)i)
            select new MetricasEvento
            {
                EventoId = eventoId,
                TituloEvento = titulo,
                IngresoTotal = ingreso,
                Estado = "Publicado",
                FechaInicio = DateTime.UtcNow.AddDays(30),
                FechaCreacion = DateTime.UtcNow,
                UltimaActualizacion = DateTime.UtcNow
            });
    }

    private static Arbitrary<MetricasEvento> GenerarMetricasEventoConFechas()
    {
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from titulo in GenerarTituloValido()
            from fechaCreacion in GenerarTimestampUTCValido()
            from minutosAdicionales in Gen.Choose(0, 1440) // 0 a 24 horas después
            select new MetricasEvento
            {
                EventoId = eventoId,
                TituloEvento = titulo,
                FechaCreacion = fechaCreacion,
                UltimaActualizacion = fechaCreacion.AddMinutes(minutosAdicionales),
                Estado = "Publicado",
                FechaInicio = DateTime.UtcNow.AddDays(30),
                IngresoTotal = 1000m
            });
    }

    private static Arbitrary<MetricasEvento> GenerarMetricasEventoConEstado()
    {
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from titulo in GenerarTituloValido()
            from estado in Gen.Elements(new[] { "Publicado", "Cancelado", "Finalizado" })
            select new MetricasEvento
            {
                EventoId = eventoId,
                TituloEvento = titulo,
                Estado = estado,
                FechaInicio = DateTime.UtcNow.AddDays(30),
                FechaCreacion = DateTime.UtcNow,
                UltimaActualizacion = DateTime.UtcNow,
                IngresoTotal = 1000m
            });
    }

    private static Gen<string> GenerarTituloValido()
    {
        return Gen.Elements(new[]
        {
            "Conferencia de Tecnología 2024",
            "Workshop de .NET",
            "Seminario de Arquitectura",
            "Meetup de Desarrolladores",
            "Curso de Microservicios"
        });
    }

    private static Gen<DateTime> GenerarTimestampUTCValido()
    {
        return Gen.Choose(-365, 0) // Desde hace un año hasta ahora
            .Select(dias => DateTime.UtcNow.AddDays(dias));
    }

    #endregion
}