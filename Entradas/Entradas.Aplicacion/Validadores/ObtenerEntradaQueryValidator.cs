using FluentValidation;
using Entradas.Aplicacion.Queries;

namespace Entradas.Aplicacion.Validadores;

/// <summary>
/// Validador para ObtenerEntradaQuery usando FluentValidation
/// </summary>
public class ObtenerEntradaQueryValidator : AbstractValidator<ObtenerEntradaQuery>
{
    public ObtenerEntradaQueryValidator()
    {
        RuleFor(x => x.EntradaId)
            .NotEmpty()
            .WithMessage("El ID de la entrada es requerido")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la entrada debe ser un GUID válido");
    }
}