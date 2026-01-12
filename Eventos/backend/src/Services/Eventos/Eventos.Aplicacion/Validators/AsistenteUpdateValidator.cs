namespace Eventos.Aplicacion.Validators;

using FluentValidation;
using Eventos.Aplicacion.DTOs;

public class AsistenteUpdateValidator : AbstractValidator<AsistenteUpdateDto>
{
 public AsistenteUpdateValidator()
 {
 RuleFor(x => x.Nombre).NotEmpty().When(x => x.Nombre is not null);
 RuleFor(x => x.Correo).EmailAddress().When(x => x.Correo is not null);
 }
}
