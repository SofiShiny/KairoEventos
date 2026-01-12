using FluentValidation;
using Usuarios.Application.Dtos;

namespace Usuarios.Application.Validators;

public class RegistroTokenValidator: AbstractValidator<RegistroTokenDto>
{
    public RegistroTokenValidator()
    {
        RuleFor(b => b.Token).NotEmpty().WithMessage("El token es requerido").Matches("(^[\\w-]*\\.[\\w-]*\\.[\\w-]*$)").WithMessage("El token debe ser valido");
    }
}