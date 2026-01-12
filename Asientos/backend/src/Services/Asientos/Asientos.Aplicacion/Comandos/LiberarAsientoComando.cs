using MediatR;
using System;

namespace Asientos.Aplicacion.Comandos;

public record LiberarAsientoComando(Guid MapaId, Guid AsientoId) : IRequest;
