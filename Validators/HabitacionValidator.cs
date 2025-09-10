using FluentValidation;
using StayGo.Models;

public class HabitacionValidator : AbstractValidator<Habitacion>
{
    public HabitacionValidator()
    {
        RuleFor(h => h.Nombre).NotEmpty().WithMessage("Nombre de habitación obligatorio.");
        RuleFor(h => h.Capacidad).GreaterThan(0).WithMessage("Capacidad debe ser > 0.");
        RuleFor(h => h.PrecioPorNoche).GreaterThan(0).WithMessage("Precio por noche debe ser > 0.");
    }
}
