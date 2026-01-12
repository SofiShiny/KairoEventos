namespace Eventos.Aplicacion.Validators;

using FluentValidation;
using Eventos.Aplicacion.DTOs;

public class EventoUpdateValidator : AbstractValidator<EventoUpdateDto>
{
 public EventoUpdateValidator()
 {
 // Validar orden de fechas si ambas están presentes usando Custom para evitar warnings de nullabilidad
 RuleFor(x => x).Custom((dto, context) =>
 {
 if (dto.FechaInicio.HasValue && dto.FechaFin.HasValue)
 {
 var inicio = dto.FechaInicio.GetValueOrDefault();
 var fin = dto.FechaFin.GetValueOrDefault();

 if (inicio >= fin)
 {
 context.AddFailure("FechaInicio", "FechaInicio debe ser anterior a FechaFin.");
 }
 }
 });

 // Si se proporciona MaximoAsistentes, debe ser mayor que0
 RuleFor(x => x.MaximoAsistentes)
 .Must(v => !v.HasValue || v.GetValueOrDefault() >0)
 .WithMessage("MaximoAsistentes debe ser mayor que0");

 // Si se proporciona titulo, no puede estar vacío
 RuleFor(x => x.Titulo).NotEmpty().When(x => x.Titulo is not null);
 }
}
