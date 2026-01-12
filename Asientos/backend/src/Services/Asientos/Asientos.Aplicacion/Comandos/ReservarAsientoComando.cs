using MediatR;
using System;

namespace Asientos.Aplicacion.Comandos;

public record ReservarAsientoComando(Guid MapaId, Guid AsientoId, Guid UsuarioId) : IRequest;
