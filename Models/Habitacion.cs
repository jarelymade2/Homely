using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using StayGo.Models.Enums;

namespace StayGo.Models;
public class Habitacion
{
    public Guid Id { get; set; }
    public Guid PropiedadId { get; set; }
    [ValidateNever]  
    public Propiedad? Propiedad { get; set; } = null!;
    public string Nombre { get; set; } = ""; // ej: "Doble 201"
    public int Capacidad { get; set; }
    public decimal PrecioPorNoche { get; set; }
    public int? Piso { get; set; }
    public string? Numero { get; set; }
    public TipoCama? TipoCama { get; set; }

    public ICollection<Disponibilidad> Disponibilidades { get; set; } = new List<Disponibilidad>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
