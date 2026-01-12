namespace Eventos.Aplicacion.Validators;

using FluentValidation;
using Eventos.Aplicacion.DTOs;

public class EventoCreateValidator : AbstractValidator<EventoCreateDto>
{
    public EventoCreateValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty();
        RuleFor(x => x.FechaInicio).LessThan(x => x.FechaFin)
            .WithMessage("FechaInicio debe ser anterior a FechaFin.");
        RuleFor(x => x.MaximoAsistentes).GreaterThan(0);

        // Validar asistentes si se proporcionan
        When(x => x.Asistentes != null && x.Asistentes.Count > 0, () =>
        {
            RuleForEach(x => x.Asistentes!).SetValidator(new AsistenteCreateValidator());
        });

        // Validar ubicacion opcionalmente usando ChildRules para evitar advertencias de nullabilidad
        When(x => x.Ubicacion != null, () =>
        {
            RuleFor(x => x.Ubicacion!).ChildRules(ubicacion =>
            {
                ubicacion.RuleFor(u => u.NombreLugar).NotEmpty();
                ubicacion.RuleFor(u => u.Ciudad).NotEmpty();
            });
        });
    }
}