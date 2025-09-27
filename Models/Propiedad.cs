using StayGo.Models.Enums;
using StayGo.Models.ValueObjects;

namespace StayGo.Models;
public class Propiedad
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public TipoPropiedad Tipo { get; set; }
    public decimal? PrecioPorNoche { get; set; } // null si es Hotel
    public int Capacidad { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public Direccion Direccion { get; set; } = new();

    public ICollection<Habitacion> Habitaciones { get; set; } = new List<Habitacion>();
    public ICollection<ImagenPropiedad> Imagenes { get; set; } = new List<ImagenPropiedad>();
    public ICollection<PropiedadAmenidad> PropiedadAmenidades { get; set; } = new List<PropiedadAmenidad>();
    public ICollection<Disponibilidad> Disponibilidades { get; set; } = new List<Disponibilidad>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Resena> Resenas { get; set; } = new List<Resena>();
    public ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
}
