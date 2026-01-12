using FluentValidation;
using Entradas.API.DTOs;

namespace Entradas.API.Validators;

/// <summary>
/// Validador para el DTO de creación de entradas
/// </summary>
public class CrearEntradaRequestDtoValidator : AbstractValidator<CrearEntradaRequestDto>
{
    public CrearEntradaRequestDtoValidator()
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

        RuleFor(x => x.AsientoId)
            .Must(asientoId => !asientoId.HasValue || asientoId.Value != Guid.Empty)
            .WithMessage("Si se especifica un asiento, debe ser un GUID válido");

    }
}