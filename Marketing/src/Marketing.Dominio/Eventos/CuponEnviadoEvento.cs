namespace Marketing.Dominio.Eventos;

public record CuponEnviadoEvento(
    Guid CuponId,
    string Codigo,
    Guid UsuarioId, // Destinatario
    decimal Valor,
    string TipoDescuento
);
