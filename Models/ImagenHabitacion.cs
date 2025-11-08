using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StayGo.Models;

public class ImagenHabitacion
{
    public Guid Id { get; set; }
    public Guid HabitacionId { get; set; }
    
    [ValidateNever]
    public Habitacion? Habitacion { get; set; } = null!;
    
    public string Url { get; set; } = "";
    public bool EsPrincipal { get; set; }
}

