using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using Newtonsoft.Json;
using Eventos.Dominio.EventosDeDominio;
using Reportes.Dominio.ModelosLectura;

namespace Reportes.Pruebas.Aplicacion.Consumers;

/// <summary>
/// Tests de propiedades para validar que la deserialización de eventos funciona correctamente
/// con cualquier evento válido generado por FsCheck.
/// 
/// Feature: Deserialización de Eventos
/// Property: Cualquier evento válido se deserializa correctamente sin pérdida de datos
/// </summary>
public class DeserializacionPropiedadesTests
{
    /// <summary>
    /// Propiedad: Cualquier EventoPublicadoEventoDominio válido se serializa y deserializa sin pérdida de datos
    /// </summary>
    [Property(MaxTest = 100)]
    public Property EventoPublicado_SerializacionDeserializacion_ConservaTodasLasPropiedades()
    {
        return Prop.ForAll(GenerarEventoPublicadoValido(), evento =>
        {
            // Act
            var json = JsonConvert.SerializeObject(evento);
            var eventoDeserializado = JsonConvert.DeserializeObject<EventoPublicadoEventoDominio>(json);

            // Assert
            return (eventoDeserializado != null &&
                    eventoDeserializado.EventoId == evento.EventoId &&
                    eventoDeserializado.TituloEvento == evento.TituloEvento &&
                    eventoDeserializado.FechaInicio == evento.FechaInicio)
                .ToProperty()
                .Label($"Evento original: {evento}, Deserializado: {eventoDeserializado}");
        });
    }

    /// <summary>
    /// Propiedad: Cualquier AsistenteRegistradoEventoDominio válido se serializa y deserializa sin pérdida de datos
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AsistenteRegistrado_SerializacionDeserializacion_ConservaTodasLasPropiedades()
    {
        return Prop.ForAll(GenerarAsistenteRegistradoValido(), evento =>
        {
            // Act
            var json = JsonConvert.SerializeObject(evento);
            var eventoDeserializado = JsonConvert.DeserializeObject<AsistenteRegistradoEventoDominio>(json);

            // Assert
            return (eventoDeserializado != null &&
                    eventoDeserializado.EventoId == evento.EventoId &&
                    eventoDeserializado.UsuarioId == evento.UsuarioId &&
                    eventoDeserializado.NombreUsuario == evento.NombreUsuario)
                .ToProperty()
                .Label($"Evento original: {evento}, Deserializado: {eventoDeserializado}");
        });
    }

    /// <summary>
    /// Propiedad: Cualquier EventoCanceladoEventoDominio válido se serializa y deserializa sin pérdida de datos
    /// </summary>
    [Property(MaxTest = 100)]
    public Property EventoCancelado_SerializacionDeserializacion_ConservaTodasLasPropiedades()
    {
        return Prop.ForAll(GenerarEventoCanceladoValido(), evento =>
        {
            // Act
            var json = JsonConvert.SerializeObject(evento);
            var eventoDeserializado = JsonConvert.DeserializeObject<EventoCanceladoEventoDominio>(json);

            // Assert
            return (eventoDeserializado != null &&
                    eventoDeserializado.EventoId == evento.EventoId &&
                    eventoDeserializado.TituloEvento == evento.TituloEvento)
                .ToProperty()
                .Label($"Evento original: {evento}, Deserializado: {eventoDeserializado}");
        });
    }

    /// <summary>
    /// Propiedad: Los eventos deserializados mantienen tipos de datos correctos
    /// </summary>
    [Property(MaxTest = 100)]
    public Property EventosDeserializados_MantienenTiposDeDatosCorrectos()
    {
        return Prop.ForAll(GenerarEventoPublicadoValido(), evento =>
        {
            // Act
            var json = JsonConvert.SerializeObject(evento);
            var eventoDeserializado = JsonConvert.DeserializeObject<EventoPublicadoEventoDominio>(json);

            // Assert
            return (eventoDeserializado != null &&
                    eventoDeserializado.EventoId.GetType() == typeof(Guid) &&
                    eventoDeserializado.TituloEvento.GetType() == typeof(string) &&
                    eventoDeserializado.FechaInicio.GetType() == typeof(DateTime))
                .ToProperty()
                .Label($"Tipos verificados para evento: {eventoDeserializado}");
        });
    }

    /// <summary>
    /// Propiedad: La deserialización es idempotente (deserializar dos veces da el mismo resultado)
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Deserializacion_EsIdempotente()
    {
        return Prop.ForAll(GenerarEventoPublicadoValido(), evento =>
        {
            // Act
            var json = JsonConvert.SerializeObject(evento);
            var primeraDeserializacion = JsonConvert.DeserializeObject<EventoPublicadoEventoDominio>(json);
            var jsonSegundo = JsonConvert.SerializeObject(primeraDeserializacion);
            var segundaDeserializacion = JsonConvert.DeserializeObject<EventoPublicadoEventoDominio>(jsonSegundo);

            // Assert
            return (primeraDeserializacion != null && segundaDeserializacion != null &&
                    primeraDeserializacion.EventoId == segundaDeserializacion.EventoId &&
                    primeraDeserializacion.TituloEvento == segundaDeserializacion.TituloEvento &&
                    primeraDeserializacion.FechaInicio == segundaDeserializacion.FechaInicio)
                .ToProperty()
                .Label($"Primera: {primeraDeserializacion}, Segunda: {segundaDeserializacion}");
        });
    }

    #region Generadores

    private static Arbitrary<EventoPublicadoEventoDominio> GenerarEventoPublicadoValido()
    {
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from titulo in GenerarTituloValido()
            from fecha in GenerarFechaValida()
            select new EventoPublicadoEventoDominio
            {
                EventoId = eventoId,
                TituloEvento = titulo,
                FechaInicio = fecha
            });
    }

    private static Arbitrary<AsistenteRegistradoEventoDominio> GenerarAsistenteRegistradoValido()
    {
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from usuarioId in GenerarUsuarioIdValido()
            from nombreUsuario in GenerarNombreUsuarioValido()
            select new AsistenteRegistradoEventoDominio
            {
                EventoId = eventoId,
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario
            });
    }

    private static Arbitrary<EventoCanceladoEventoDominio> GenerarEventoCanceladoValido()
    {
        return Arb.From(
            from eventoId in Arb.Generate<Guid>()
            from titulo in GenerarTituloValido()
            select new EventoCanceladoEventoDominio
            {
                EventoId = eventoId,
                TituloEvento = titulo
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
            "Curso de Microservicios",
            "Evento de Networking",
            "Hackathon 2024",
            "Charla sobre Cloud Computing"
        });
    }

    private static Gen<string> GenerarUsuarioIdValido()
    {
        return Gen.Elements(new[]
        {
            "user-123",
            "admin-456",
            "guest-789",
            "developer-001",
            "manager-002",
            "analyst-003"
        });
    }

    private static Gen<string> GenerarNombreUsuarioValido()
    {
        return Gen.Elements(new[]
        {
            "Juan Pérez",
            "María García",
            "Carlos López",
            "Ana Martínez",
            "Luis Rodríguez",
            "Carmen Sánchez",
            "José González",
            "Laura Fernández"
        });
    }

    private static Gen<DateTime> GenerarFechaValida()
    {
        return Gen.Choose(0, 365)
            .Select(dias => DateTime.UtcNow.AddDays(dias));
    }

    #endregion
}