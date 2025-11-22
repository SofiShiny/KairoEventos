using Eventos.Aplicacion.Comandos;
using FluentValidation;

namespace Eventos.Aplicacion.Validators;

public class CrearEventoComandoValidator : AbstractValidator<CrearEventoComando>
{
 public CrearEventoComandoValidator()
 {
 RuleFor(x => x.Ubicacion).NotNull().WithMessage("La ubicacion es obligatoria");
 RuleFor(x => x.Titulo).NotEmpty().WithMessage("El titulo es obligatorio");
 RuleFor(x => x.OrganizadorId).NotEmpty().WithMessage("El organizador es obligatorio");
 RuleFor(x => x.MaximoAsistentes).GreaterThan(0).WithMessage("El maximo de asistentes debe ser mayor que cero");
 RuleFor(x => x.FechaFin).GreaterThan(x => x.FechaInicio).WithMessage("La fecha fin debe ser posterior a la fecha inicio");
 When(x => x.Ubicacion != null, () =>
 {
 RuleFor(x => x.Ubicacion.NombreLugar).NotEmpty().WithMessage("El nombre del lugar es obligatorio");
 RuleFor(x => x.Ubicacion.Direccion).NotEmpty().WithMessage("La direccion es obligatoria");
 RuleFor(x => x.Ubicacion.Ciudad).NotEmpty().WithMessage("La ciudad es obligatoria");
 RuleFor(x => x.Ubicacion.Pais).NotEmpty().WithMessage("El pais es obligatorio");
 });
 }
}
