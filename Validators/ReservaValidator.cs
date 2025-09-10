using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;

public class ReservaValidator : AbstractValidator<Reserva>
{
    public ReservaValidator(StayGoContext ctx)
    {
        RuleFor(r => r.CheckIn)
            .LessThan(r => r.CheckOut).WithMessage("El Check-in debe ser antes del Check-out.");

        RuleFor(r => r.Huespedes)
            .GreaterThan(0).WithMessage("Debe haber al menos 1 huésped.");

        // Si la propiedad es Hotel => HabitacionId obligatorio
        RuleFor(r => r.HabitacionId)
            .NotNull().WithMessage("Las reservas en hoteles requieren seleccionar una habitación.")
            .WhenAsync(async (r, ct) =>
            {
                var tipo = await ctx.Propiedades
                    .Where(p => p.Id == r.PropiedadId)
                    .Select(p => p.Tipo)
                    .FirstOrDefaultAsync(ct);

                return tipo == TipoPropiedad.Hotel;
            });

        // (Opcional) Evitar fechas pasadas
        RuleFor(r => r.CheckIn)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("El Check-in no puede ser en el pasado.");
    }
}
