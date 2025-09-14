using FluentValidation;
using StayGo.Models;

public class ContactoMensajeValidator : AbstractValidator<ContactoMensaje>
{
    public ContactoMensajeValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Mensaje).NotEmpty().MaximumLength(1000);
    }
}
