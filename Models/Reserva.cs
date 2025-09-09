namespace StayGo.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        // FK -> Propiedad
        public int PropiedadId { get; set; }
        public Propiedad? Propiedad { get; set; }  // <- usada en Include()

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string? ClienteNombre { get; set; }
    }
}
