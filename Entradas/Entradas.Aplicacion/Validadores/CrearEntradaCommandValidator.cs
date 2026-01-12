using FluentValidation;
using Entradas.Aplicacion.Comandos;

namespace Entradas.Aplicacion.Validadores;

/// <summary>
/// Validador para el comando CrearEntradaCommand usando FluentValidation
/// </summary>
public class CrearEntradaCommandValidator : AbstractValidator<CrearEntradaCommand>
{
    public CrearEntradaCommandValidator()
    {
        RuleFor(x => x.EventoId)
            .NotEmpty()
            .WithMessage("El ID del evento es requerido")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del evento debe ser un GUID válido");

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del usuario debe ser un GUID válido");


        // AsientoId es opcional, pero si se proporciona debe ser válido
        RuleFor(x => x.AsientoId)
            .NotEqual(Guid.Empty)
            .When(x => x.AsientoId.HasValue)
            .WithMessage("Si se especifica un asiento, debe ser un GUID válido");
    }
}