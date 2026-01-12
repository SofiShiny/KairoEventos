using FluentValidation;
using Entradas.Aplicacion.Queries;

namespace Entradas.Aplicacion.Validadores;

/// <summary>
/// Validador para ObtenerEntradasPorUsuarioQuery usando FluentValidation
/// </summary>
public class ObtenerEntradasPorUsuarioQueryValidator : AbstractValidator<ObtenerEntradasPorUsuarioQuery>
{
    public ObtenerEntradasPorUsuarioQueryValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del usuario debe ser un GUID válido");
    }
}