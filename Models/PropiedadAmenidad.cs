namespace StayGo.Models;
public class PropiedadAmenidad
{
    public Guid PropiedadId { get; set; }
    public Propiedad Propiedad { get; set; } = null!;
    public int AmenidadId { get; set; }
    public Amenidad Amenidad { get; set; } = null!;
}
