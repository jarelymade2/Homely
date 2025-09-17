namespace StayGo.Models;

public class Resena
{
    public Guid Id { get; set; }
    public Guid PropiedadId { get; set; }
    public Propiedad Propiedad { get; set; } = null!;
    
    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;
    
    public int Puntuacion { get; set; }
    public string Comentario { get; set; } = "";
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}