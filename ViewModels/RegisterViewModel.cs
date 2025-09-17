using System.ComponentModel.DataAnnotations;

namespace StayGo.ViewModels;

public class RegisterViewModel
{
    // Agrega estas líneas
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombres")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [Display(Name = "Apellidos")]
    public string LastName { get; set; } = "";
    
    // ... el resto de tus propiedades ...
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Phone(ErrorMessage = "El teléfono no es válido.")]
    public string PhoneNumber { get; set; } = "";

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    public string Email { get; set; } = "";
    
    // ... y el resto de tu modelo ...
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirma la contraseña.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
    public string ConfirmPassword { get; set; } = "";
}