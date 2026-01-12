using Asientos.Dominio.EventosDominio;
using Eventos.Dominio.EventosDeDominio;
using FsCheck;
using FsCheck.Xunit;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Reportes.Aplicacion.Consumers;
using Reportes.Dominio.ModelosLectura;
using Reportes.Dominio.Repositorios;

namespace Reportes.Pruebas.Aplicacion.Consumers;

/// <summary>
/// Property-Based Tests para consumidores de eventos del microservicio de Reportes.
/// Cada test ejecuta 100 iteraciones con datos generados aleatoriamente.
/// </summary>
public class ConsumidoresPropiedadesTests
{
    /// <summary>
    /// Feature: microservicio-reportes, Property 2: Incremento atómico de contadores
    /// Valida: Requisitos 1.2
    /// 
    /// Para cualquier evento AsistenteRegistradoEventoDominio, el contador 
    /// TotalAsistentesRegistrados en HistorialAsistencia debe incrementarse exactamente en 1.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(GeneradoresEventos) })]
    public Property Propiedad2_IncrementoAtomicoContadores(
        AsistenteRegistradoEventoDominio evento,
        int contadorInicial)
    {
        // Arrange
        var repositorioMock = new Mock<IRepositorioReportesLectura>();
        var loggerMock = new Mock<ILogger<AsistenteRegistradoConsumer>>();

        // Asegurar que el contador inicial sea no negativo
        contadorInicial = Math.Abs(contadorInicial) % 1000;

        var historialInicial = new HistorialAsistencia
        {
            EventoId = evento.EventoId,
            TotalAsistentesRegistrados = contadorInicial,
            Asistentes = new List<RegistroAsistente>(),
            UltimaActualizacion = DateTime.UtcNow
        };

        HistorialAsistencia? historialActualizado = null;

        repositorioMock
            .Setup(r => r.ObtenerAsistenciaEventoAsync(evento.EventoId))
            .ReturnsAsync(historialInicial);

        repositorioMock
            .Setup(r => r.ActualizarAsistenciaAsync(It.IsAny<HistorialAsistencia>()))
            .Callback<HistorialAsistencia>(h => historialActualizado = h)
            .Returns(Task.CompletedTask);

        repositorioMock
            .Setup(r => r.ObtenerMetricasEventoAsync(evento.EventoId))
            .ReturnsAsync((MetricasEvento?)null);

        repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .Returns(Task.CompletedTask);

        var consumer = new AsistenteRegistradoConsumer(repositorioMock.Object, loggerMock.Object);
        var contextMock = CreateConsumeContext(evento);

        // Act
        consumer.Consume(contextMock.Object).Wait();

        // Assert
        var incrementoEsperado = 1;
        var contadorFinal = historialActualizado?.TotalAsistentesRegistrados ?? contadorInicial;
        var incrementoReal = contadorFinal - contadorInicial;

        return (incrementoReal == incrementoEsperado)
            .ToProperty()
            .Label($"Contador inicial: {contadorInicial}, " +
                   $"Contador final: {contadorFinal}, " +
                   $"Incremento esperado: {incrementoEsperado}, " +
                   $"Incremento real: {incrementoReal}");
    }

    /// <summary>
    /// Feature: microservicio-reportes, Property 4: Auditoría completa de operaciones
    /// Valida: Requisitos 1.5
    /// 
    /// Para cualquier evento procesado exitosamente, debe existir un registro 
    /// correspondiente en la colección LogAuditoria con Exitoso = true.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(GeneradoresEventos) })]
    public Property Propiedad4_AuditoriaCompletaOperaciones_EventoPublicado(
        EventoPublicadoEventoDominio evento)
    {
        // Arrange
        var repositorioMock = new Mock<IRepositorioReportesLectura>();
        var loggerMock = new Mock<ILogger<EventoPublicadoConsumer>>();

        LogAuditoria? logRegistrado = null;

        repositorioMock
            .Setup(r => r.ObtenerMetricasEventoAsync(evento.EventoId))
            .ReturnsAsync((MetricasEvento?)null);

        repositorioMock
            .Setup(r => r.ActualizarMetricasAsync(It.IsAny<MetricasEvento>()))
            .Returns(Task.CompletedTask);

        repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .Callback<LogAuditoria>(log => logRegistrado = log)
            .Returns(Task.CompletedTask);

        var consumer = new EventoPublicadoConsumer(repositorioMock.Object, loggerMock.Object);
        var contextMock = CreateConsumeContext(evento);

        // Act
        consumer.Consume(contextMock.Object).Wait();

        // Assert
        var logExiste = logRegistrado != null;
        var logExitoso = logRegistrado?.Exitoso ?? false;
        var tipoOperacionCorrecto = logRegistrado?.TipoOperacion == "EventoConsumido";
        var entidadCorrecta = logRegistrado?.Entidad == "Evento";
        var entidadIdCorrecto = logRegistrado?.EntidadId == evento.EventoId.ToString();

        return (logExiste && logExitoso && tipoOperacionCorrecto && entidadCorrecta && entidadIdCorrecto)
            .ToProperty()
            .Label($"Log existe: {logExiste}, " +
                   $"Log exitoso: {logExitoso}, " +
                   $"Tipo operación correcto: {tipoOperacionCorrecto}, " +
                   $"Entidad correcta: {entidadCorrecta}, " +
                   $"EntidadId correcto: {entidadIdCorrecto}");
    }

    /// <summary>
    /// Feature: microservicio-reportes, Property 4: Auditoría completa de operaciones
    /// Valida: Requisitos 1.5
    /// 
    /// Para cualquier evento AsistenteRegistrado procesado exitosamente, 
    /// debe existir un registro en LogAuditoria con Exitoso = true.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(GeneradoresEventos) })]
    public Property Propiedad4_AuditoriaCompletaOperaciones_AsistenteRegistrado(
        AsistenteRegistradoEventoDominio evento)
    {
        // Arrange
        var repositorioMock = new Mock<IRepositorioReportesLectura>();
        var loggerMock = new Mock<ILogger<AsistenteRegistradoConsumer>>();

        LogAuditoria? logRegistrado = null;

        var historial = new HistorialAsistencia
        {
            EventoId = evento.EventoId,
            TotalAsistentesRegistrados = 0,
            Asistentes = new List<RegistroAsistente>(),
            UltimaActualizacion = DateTime.UtcNow
        };

        repositorioMock
            .Setup(r => r.ObtenerAsistenciaEventoAsync(evento.EventoId))
            .ReturnsAsync(historial);

        repositorioMock
            .Setup(r => r.ActualizarAsistenciaAsync(It.IsAny<HistorialAsistencia>()))
            .Returns(Task.CompletedTask);

        repositorioMock
            .Setup(r => r.ObtenerMetricasEventoAsync(evento.EventoId))
            .ReturnsAsync((MetricasEvento?)null);

        repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .Callback<LogAuditoria>(log => logRegistrado = log)
            .Returns(Task.CompletedTask);

        var consumer = new AsistenteRegistradoConsumer(repositorioMock.Object, loggerMock.Object);
        var contextMock = CreateConsumeContext(evento);

        // Act
        consumer.Consume(contextMock.Object).Wait();

        // Assert
        var logExiste = logRegistrado != null;
        var logExitoso = logRegistrado?.Exitoso ?? false;
        var tipoOperacionCorrecto = logRegistrado?.TipoOperacion == "EventoConsumido";
        var entidadCorrecta = logRegistrado?.Entidad == "Asistente";

        return (logExiste && logExitoso && tipoOperacionCorrecto && entidadCorrecta)
            .ToProperty()
            .Label($"Log existe: {logExiste}, " +
                   $"Log exitoso: {logExitoso}, " +
                   $"Tipo operación correcto: {tipoOperacionCorrecto}, " +
                   $"Entidad correcta: {entidadCorrecta}");
    }

    /// <summary>
    /// Feature: microservicio-reportes, Property 4: Auditoría completa de operaciones
    /// Valida: Requisitos 1.5
    /// 
    /// Para cualquier evento AsientoReservado procesado exitosamente, 
    /// debe existir un registro en LogAuditoria con Exitoso = true.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(GeneradoresEventos) })]
    public Property Propiedad4_AuditoriaCompletaOperaciones_AsientoReservado(
        AsientoReservadoEventoDominio evento)
    {
        // Arrange
        var repositorioMock = new Mock<IRepositorioReportesLectura>();
        var loggerMock = new Mock<ILogger<AsientoReservadoConsumer>>();

        LogAuditoria? logRegistrado = null;

        repositorioMock
            .Setup(r => r.ObtenerVentasDiariasAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((ReporteVentasDiarias?)null);

        repositorioMock
            .Setup(r => r.ActualizarVentasDiariasAsync(It.IsAny<ReporteVentasDiarias>()))
            .Returns(Task.CompletedTask);

        repositorioMock
            .Setup(r => r.ObtenerAsistenciaEventoAsync(evento.MapaId))
            .ReturnsAsync((HistorialAsistencia?)null);

        repositorioMock
            .Setup(r => r.ObtenerMetricasEventoAsync(evento.MapaId))
            .ReturnsAsync((MetricasEvento?)null);

        repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .Callback<LogAuditoria>(log => logRegistrado = log)
            .Returns(Task.CompletedTask);

        var consumer = new AsientoReservadoConsumer(repositorioMock.Object, loggerMock.Object);
        var contextMock = CreateConsumeContext(evento);

        // Act
        consumer.Consume(contextMock.Object).Wait();

        // Assert
        var logExiste = logRegistrado != null;
        var logExitoso = logRegistrado?.Exitoso ?? false;
        var tipoOperacionCorrecto = logRegistrado?.TipoOperacion == "EventoConsumido";
        var entidadCorrecta = logRegistrado?.Entidad == "Asiento";

        return (logExiste && logExitoso && tipoOperacionCorrecto && entidadCorrecta)
            .ToProperty()
            .Label($"Log existe: {logExiste}, " +
                   $"Log exitoso: {logExitoso}, " +
                   $"Tipo operación correcto: {tipoOperacionCorrecto}, " +
                   $"Entidad correcta: {entidadCorrecta}");
    }

    /// <summary>
    /// Feature: microservicio-reportes, Property 4: Auditoría completa de operaciones
    /// Valida: Requisitos 1.5
    /// 
    /// Para cualquier evento AsientoLiberado procesado exitosamente, 
    /// debe existir un registro en LogAuditoria con Exitoso = true.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(GeneradoresEventos) })]
    public Property Propiedad4_AuditoriaCompletaOperaciones_AsientoLiberado(
        AsientoLiberadoEventoDominio evento)
    {
        // Arrange
        var repositorioMock = new Mock<IRepositorioReportesLectura>();
        var loggerMock = new Mock<ILogger<AsientoLiberadoConsumer>>();

        LogAuditoria? logRegistrado = null;

        repositorioMock
            .Setup(r => r.ObtenerAsistenciaEventoAsync(evento.MapaId))
            .ReturnsAsync((HistorialAsistencia?)null);

        repositorioMock
            .Setup(r => r.RegistrarLogAuditoriaAsync(It.IsAny<LogAuditoria>()))
            .Callback<LogAuditoria>(log => logRegistrado = log)
            .Returns(Task.CompletedTask);

        var consumer = new AsientoLiberadoConsumer(repositorioMock.Object, loggerMock.Object);
        var contextMock = CreateConsumeContext(evento);

        // Act
        consumer.Consume(contextMock.Object).Wait();

        // Assert
        var logExiste = logRegistrado != null;
        var logExitoso = logRegistrado?.Exitoso ?? false;
        var tipoOperacionCorrecto = logRegistrado?.TipoOperacion == "EventoConsumido";
        var entidadCorrecta = logRegistrado?.Entidad == "Asiento";

        return (logExiste && logExitoso && tipoOperacionCorrecto && entidadCorrecta)
            .ToProperty()
            .Label($"Log existe: {logExiste}, " +
                   $"Log exitoso: {logExitoso}, " +
                   $"Tipo operación correcto: {tipoOperacionCorrecto}, " +
                   $"Entidad correcta: {entidadCorrecta}");
    }

    // Helper method to create ConsumeContext mock
    private static Mock<ConsumeContext<T>> CreateConsumeContext<T>(T message) where T : class
    {
        var contextMock = new Mock<ConsumeContext<T>>();
        contextMock.Setup(c => c.Message).Returns(message);
        return contextMock;
    }
}

/// <summary>
/// Generadores personalizados para FsCheck que crean instancias válidas de eventos de dominio.
/// </summary>
public static class GeneradoresEventos
{
    /// <summary>
    /// Genera instancias válidas de EventoPublicadoEventoDominio con datos aleatorios.
    /// </summary>
    public static Arbitrary<EventoPublicadoEventoDominio> GeneradorEventoPublicado()
    {
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from titulo in Arb.Generate<NonEmptyString>()
            from fecha in Arb.Generate<DateTime>()
            select new EventoPublicadoEventoDominio
            {
                EventoId = eventoId,
                TituloEvento = titulo.Get,
                FechaInicio = fecha
            });
    }

    /// <summary>
    /// Genera instancias válidas de AsistenteRegistradoEventoDominio con datos aleatorios.
    /// </summary>
    public static Arbitrary<AsistenteRegistradoEventoDominio> GeneradorAsistenteRegistrado()
    {
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from usuarioId in Arb.Generate<NonEmptyString>()
            from nombreUsuario in Arb.Generate<NonEmptyString>()
            select new AsistenteRegistradoEventoDominio
            {
                EventoId = eventoId,
                UsuarioId = usuarioId.Get,
                NombreUsuario = nombreUsuario.Get
            });
    }

    /// <summary>
    /// Genera instancias válidas de AsientoReservadoEventoDominio con datos aleatorios.
    /// </summary>
    public static Arbitrary<AsientoReservadoEventoDominio> GeneradorAsientoReservado()
    {
        return Arb.From(
            from mapaId in Arb.Generate<Guid>()
            from fila in Gen.Choose(1, 50)
            from numero in Gen.Choose(1, 100)
            select new AsientoReservadoEventoDominio
            {
                MapaId = mapaId,
                Fila = fila,
                Numero = numero
            });
    }

    /// <summary>
    /// Genera instancias válidas de AsientoLiberadoEventoDominio con datos aleatorios.
    /// </summary>
    public static Arbitrary<AsientoLiberadoEventoDominio> GeneradorAsientoLiberado()
    {
        return Arb.From(
            from mapaId in Arb.Generate<Guid>()
            from fila in Gen.Choose(1, 50)
            from numero in Gen.Choose(1, 100)
            select new AsientoLiberadoEventoDominio
            {
                MapaId = mapaId,
                Fila = fila,
                Numero = numero
            });
    }
}
