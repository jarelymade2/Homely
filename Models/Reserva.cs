using StayGo.Models.Enums;

namespace StayGo.Models;

public class Reserva
{
    public Guid Id { get; set; }
    public Guid PropiedadId { get; set; }
    public Propiedad Propiedad { get; set; } = null!;
    public Guid? HabitacionId { get; set; }
    public Habitacion? Habitacion { get; set; }
    
    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;
    
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int Huespedes { get; set; }
    public decimal PrecioTotal { get; set; }
    public EstadoReserva Estado { get; set; } = EstadoReserva.Pendiente;

    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}