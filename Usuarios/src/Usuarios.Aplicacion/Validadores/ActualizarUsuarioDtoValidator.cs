using FluentValidation;
using Usuarios.Aplicacion.DTOs;

namespace Usuarios.Aplicacion.Validadores;

public class ActualizarUsuarioDtoValidator : AbstractValidator<ActualizarUsuarioDto>
{
    public ActualizarUsuarioDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El teléfono es requerido");

        RuleFor(x => x.Direccion)
            .NotEmpty().WithMessage("La dirección es requerida")
            .MinimumLength(5).WithMessage("La dirección debe tener al menos 5 caracteres");
    }
}
