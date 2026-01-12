using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Usuarios.Application.Dtos;

namespace Usuarios.Application.Validators
{
    public class ActualizarUsuarioValidator : AbstractValidator<ActualizarUsuarioDto>
    {
        public ActualizarUsuarioValidator()
        {
            RuleFor(u => u.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
                .Length(2, 30).WithMessage("El nombre debe tener entre 2 y 50 caracteres.")
                .Matches("^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]").WithMessage("El nombre solo tendra letras y espacios"); ;
            RuleFor(u => u.Correo).NotEmpty().WithMessage("El correo es obligatorio.")
                .EmailAddress().WithMessage("El correo debe der valido");
            RuleFor(u => u.Telefono).NotEmpty().WithMessage("El telefono es obligatorio.")
                .Matches(@"^\d{11}$").WithMessage("El telefono debe tener 11 digitos.");
            RuleFor(u => u.Direccion).NotEmpty().WithMessage("La direccion es obligatoria.")
                .Length(2,50);
        }
    }
}
