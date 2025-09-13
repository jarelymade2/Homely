namespace StayGo.Models;
public class Amenidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public ICollection<PropiedadAmenidad> PropiedadAmenidades { get; set; } = new List<PropiedadAmenidad>();
}
