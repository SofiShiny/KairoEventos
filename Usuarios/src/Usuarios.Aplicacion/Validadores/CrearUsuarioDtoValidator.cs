using FluentValidation;
using Usuarios.Aplicacion.DTOs;

namespace Usuarios.Aplicacion.Validadores;

public class CrearUsuarioDtoValidator : AbstractValidator<CrearUsuarioDto>
{
    public CrearUsuarioDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El username es requerido")
            .MinimumLength(3).WithMessage("El username debe tener al menos 3 caracteres")
            .MaximumLength(50).WithMessage("El username no puede exceder 50 caracteres");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo es requerido")
            .EmailAddress().WithMessage("El correo no es válido");

        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El teléfono es requerido");

        RuleFor(x => x.Direccion)
            .NotEmpty().WithMessage("La dirección es requerida")
            .MinimumLength(5).WithMessage("La dirección debe tener al menos 5 caracteres");

        RuleFor(x => x.Rol)
            .IsInEnum().WithMessage("El rol no es válido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres");
    }
}
