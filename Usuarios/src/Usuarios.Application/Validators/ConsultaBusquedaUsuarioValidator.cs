using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Usuarios.Application.Dtos;

namespace Usuarios.Application.Validators
{
    public class ConsultaBusquedaUsuarioValidator: AbstractValidator<BusquedaUsuarioDto>
    {
        public ConsultaBusquedaUsuarioValidator()
        {
            RuleFor(b => b.Busqueda).Matches("^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]").When(b => b.Busqueda != null).WithMessage("El campo busqueda solo tendra letras y espacios");
        }
    }
}
