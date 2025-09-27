using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StayGo.Models;
public class ImagenPropiedad
{
    public Guid Id { get; set; }
    public Guid PropiedadId { get; set; }
    
     [ValidateNever]  
    public Propiedad? Propiedad { get; set; } = null!;
    public string Url { get; set; } = "";
    public bool EsPrincipal { get; set; }
}
