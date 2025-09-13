namespace StayGo.Models;
public class Favorito
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public Guid PropiedadId { get; set; }
    public Propiedad Propiedad { get; set; } = null!;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
