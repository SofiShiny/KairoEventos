using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace Usuarios.Application.Exceptions
{
    public class ValidatorException(string message) : Exception(message)
    {
        public void AgregarErrores(List<ValidationFailure>? errores)
        {
            this.Data["Errores"] = errores!.Select(e => new {Atributo = e.PropertyName, Error = e.ErrorMessage}).ToList();
        }

    }
}
