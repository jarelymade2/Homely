namespace StayGo.Models;

public class Favorito
{
    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;
    public int Id { get; set; } 
    public Guid PropiedadId { get; set; }
    public Propiedad Propiedad { get; set; } = null!;
    
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}