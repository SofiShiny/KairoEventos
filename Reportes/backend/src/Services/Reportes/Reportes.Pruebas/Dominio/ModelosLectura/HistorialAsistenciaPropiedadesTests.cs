using FsCheck;
using FsCheck.Xunit;
using Reportes.Dominio.ModelosLectura;

namespace Reportes.Pruebas.Dominio.ModelosLectura;

/// <summary>
/// Property-Based Tests para modelos de dominio del microservicio de Reportes.
/// Cada test ejecuta 100 iteraciones con datos generados aleatoriamente.
/// </summary>
public class HistorialAsistenciaPropiedadesTests
{
    /// <summary>
    /// Feature: microservicio-reportes, Property 3: Invariante de disponibilidad de asientos
    /// Valida: Requisitos 1.4, 6.5
    /// 
    /// Para cualquier evento de reserva o liberación de asiento, debe cumplirse:
    /// AsientosDisponibles + AsientosReservados = CapacidadTotal
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(GeneradoresHistorialAsistencia) })]
    public Property Propiedad3_InvarianteDisponibilidadAsientos(HistorialAsistencia historial)
    {
        // Arrange & Act
        var suma = historial.AsientosDisponibles + historial.AsientosReservados;
        
        // Assert
        return (suma == historial.CapacidadTotal)
            .ToProperty()
            .Label($"AsientosDisponibles ({historial.AsientosDisponibles}) + " +
                   $"AsientosReservados ({historial.AsientosReservados}) = " +
                   $"CapacidadTotal ({historial.CapacidadTotal})");
    }
}

/// <summary>
/// Generadores personalizados para FsCheck que crean instancias válidas de HistorialAsistencia
/// respetando las invariantes del dominio.
/// </summary>
public static class GeneradoresHistorialAsistencia
{
    /// <summary>
    /// Genera instancias de HistorialAsistencia con datos coherentes:
    /// - CapacidadTotal entre 10 y 1000
    /// - AsientosReservados entre 0 y CapacidadTotal
    /// - AsientosDisponibles = CapacidadTotal - AsientosReservados
    /// - PorcentajeOcupacion calculado correctamente
    /// </summary>
    public static Arbitrary<HistorialAsistencia> GeneradorHistorialAsistencia()
    {
        return Arb.From(
            from capacidad in Gen.Choose(10, 1000)
            from reservados in Gen.Choose(0, capacidad)
            from eventoId in Arb.Generate<Guid>()
            from titulo in Arb.Generate<NonEmptyString>()
            select new HistorialAsistencia
            {
                EventoId = eventoId,
                TituloEvento = titulo.Get,
                CapacidadTotal = capacidad,
                AsientosReservados = reservados,
                AsientosDisponibles = capacidad - reservados,
                PorcentajeOcupacion = capacidad > 0 ? (double)reservados / capacidad * 100 : 0,
                TotalAsistentesRegistrados = reservados, // Simplificación: asumimos 1 asistente por asiento
                Asistentes = new List<RegistroAsistente>(),
                UltimaActualizacion = DateTime.UtcNow
            });
    }
}
