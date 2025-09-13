using FluentValidation;
using StayGo.Models;

public class PropiedadValidator : AbstractValidator<Propiedad>
{
    public PropiedadValidator()
    {
        RuleFor(p => p.Titulo)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(200);

        RuleFor(p => p.Capacidad)
            .GreaterThan(0).WithMessage("La capacidad debe ser mayor a 0.");

        // Casa/Depa => PrecioPorNoche obligatorio (> 0)
        RuleFor(p => p.PrecioPorNoche)
            .NotNull().WithMessage("Precio por noche es obligatorio para casas/departamentos.")
            .GreaterThan(0).WithMessage("El precio por noche debe ser mayor a 0.")
            .When(p => p.Tipo == TipoPropiedad.Casa || p.Tipo == TipoPropiedad.Departamento);

        // Hotel => PrecioPorNoche == null y >= 1 habitación
        RuleFor(p => p.PrecioPorNoche)
            .Must(precio => precio == null)
            .WithMessage("El hotel no define precio por noche a nivel de propiedad (va por habitación).")
            .When(p => p.Tipo == TipoPropiedad.Hotel);

        RuleFor(p => p.Habitaciones)
            .Must(habs => habs != null && habs.Count > 0)
            .WithMessage("Un hotel debe tener al menos una habitación.")
            .When(p => p.Tipo == TipoPropiedad.Hotel);

        // (Opcional) Coordenadas válidas si vinieran
        RuleFor(p => p.Lat).InclusiveBetween(-90, 90)
            .When(p => p.Lat != 0 || p.Lng != 0)
            .WithMessage("Lat fuera de rango (-90 a 90).");

        RuleFor(p => p.Lng).InclusiveBetween(-180, 180)
            .When(p => p.Lat != 0 || p.Lng != 0)
            .WithMessage("Lng fuera de rango (-180 a 180).");
    }
}
