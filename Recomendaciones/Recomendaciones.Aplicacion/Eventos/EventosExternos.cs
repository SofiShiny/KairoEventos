namespace Recomendaciones.Aplicacion.Eventos;

// Contratos Externos (Simulados basados en la arquitectura del sistema)

public record EntradaCompradaEvento(
    Guid EntradaId,
    Guid UsuarioId,
    Guid EventoId,
    string Categoria
);

public record EventoCreadoEvento(
    Guid EventoId,
    string Titulo,
    string Categoria,
    DateTime FechaInicio
);

public record EventoCanceladoEvento(
    Guid EventoId,
    string Motivo
);
