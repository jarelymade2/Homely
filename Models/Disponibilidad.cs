namespace StayGo.Models;
public class Disponibilidad
{
    public Guid Id { get; set; }
    public Guid PropiedadId { get; set; }
    public Propiedad Propiedad { get; set; } = null!;
    public Guid? HabitacionId { get; set; }  // si hotel, por habitación
    public Habitacion? Habitacion { get; set; }
    public DateOnly Desde { get; set; }
    public DateOnly Hasta { get; set; }
}
