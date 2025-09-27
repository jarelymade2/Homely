using System.ComponentModel.DataAnnotations;

namespace StayGo.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    public string Email { get; set; } = "";
}