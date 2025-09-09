namespace StayGo.Models;
public class Resena
{
    public Guid Id { get; set; }
    public Guid PropiedadId { get; set; }
    public Propiedad Propiedad { get; set; } = null!;
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int Puntuacion { get; set; } // 1..5
    public string Comentario { get; set; } = "";
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
