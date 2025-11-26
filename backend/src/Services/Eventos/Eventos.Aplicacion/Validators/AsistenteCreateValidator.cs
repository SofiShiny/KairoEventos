namespace Eventos.Aplicacion.Validators;

using FluentValidation;
using Eventos.Aplicacion.DTOs;

public class AsistenteCreateValidator : AbstractValidator<AsistenteCreateDto>
{
 public AsistenteCreateValidator()
 {
 RuleFor(x => x.Nombre).NotEmpty().When(x => x.Nombre is not null).WithMessage("El nombre es requerido");
 RuleFor(x => x.Correo).EmailAddress().When(x => x.Correo is not null);
 }
}
