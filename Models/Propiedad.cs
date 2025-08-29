namespace StayGo.Models;
public class Propiedad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Ubicación { get; set; } = null!;
    public decimal PrecioPorNoche { get; set; }
}
