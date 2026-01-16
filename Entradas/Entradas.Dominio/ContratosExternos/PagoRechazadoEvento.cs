using System;

namespace Pagos.Aplicacion.Eventos;

public record PagoRechazadoEvento(Guid TransaccionId, Guid OrdenId, Guid UsuarioId, string Motivo);
