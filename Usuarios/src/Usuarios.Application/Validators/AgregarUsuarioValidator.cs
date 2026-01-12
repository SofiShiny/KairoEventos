using FluentValidation;
using Usuarios.Application.Dtos;

namespace Usuarios.Application.Validators
{
    public class AgregarUsuarioValidator : AbstractValidator<AgregarUsuarioDto>
    {
        private static readonly List<String> Roles = new()
        {
            "Organizador",
            "Soporte"
        };
        public AgregarUsuarioValidator()
        {
            RuleFor(u => u.Username).NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
                .Length(3, 20).WithMessage("El nombre de usuario debe tener entre 3 y 20 caracteres.");
            RuleFor(u => u.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
                .Length(2, 30).WithMessage("El nombre debe tener entre 2 y 30 caracteres.")
                .Matches("^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]").WithMessage("El nombre solo tendra letras y espacios");
            RuleFor(u => u.Correo).NotEmpty().WithMessage("El correo es obligatorio.")
                .EmailAddress().WithMessage("El correo debe ser valido.");
            RuleFor(u => u.Contrasena).NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 6 caracteres.").Matches("[A-Z]").WithMessage("Debe contener una letra mayuscula")
                .Matches("[a-z]").WithMessage("Debe tener una letra minuscula")
                .Matches("[0-9]").WithMessage("Debe tener un numero")
                .Matches("[^ a-zA-Z0-9]").WithMessage("Debe tener un caracter especial");
            RuleFor(u => u.ConfirmarContrasena).NotEmpty()
                .WithMessage("La confirmacion de la contraseña es obligatoria.")
                .Equal(u => u.Contrasena).WithMessage("La confirmacion de la contrasena debe ser igual a la contrasena");
            RuleFor(u => u.Telefono).NotEmpty().WithMessage("El telefono es obligatorio.")
                .Matches(@"^\d{11}$").WithMessage("El telefono debe tener 11 digitos.");
            RuleFor(u => u.Direccion).NotEmpty().WithMessage("La direccion es obligatoria.");
            RuleFor(u => u.Rol).Must(r => Roles.Contains(r))
                .WithMessage("El rol solo puede ser Organizador o Soporte").NotEmpty().WithMessage("El rol es obligatorio.");
        }
    }
}
