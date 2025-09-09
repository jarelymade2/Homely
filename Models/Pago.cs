using StayGo.Models.Enums;

namespace StayGo.Models;
public class Pago
{
    public Guid Id { get; set; }
    public Guid ReservaId { get; set; }
    public Reserva Reserva { get; set; } = null!;
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "PEN";
    public MetodoPago Metodo { get; set; } = MetodoPago.Yape;
    public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;
    public string? TransaccionRef { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime? ActualizadoEn { get; set; }
}
