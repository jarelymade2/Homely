namespace StayGo.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        // FK hacia Propiedad
        public int PropiedadId { get; set; }
        public Propiedad? Propiedad { get; set; }
    }
}
