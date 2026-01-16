using System;
using MediatR;

namespace Entradas.Aplicacion.Comandos;

/// <summary>
/// Comando para cancelar una entrada existente
/// </summary>
/// <param name="EntradaId">ID de la entrada a cancelar</param>
/// <param name="UsuarioId">ID del usuario que solicita la cancelación (para validación)</param>
public record CancelarEntradaCommand(Guid EntradaId, Guid UsuarioId) : IRequest<bool>;
