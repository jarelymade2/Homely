using System.ComponentModel.DataAnnotations;

namespace StayGo.ViewModels.Admin
{
    public class DisponibilidadViewModel
    {
        public Guid PropiedadId { get; set; }
        public string PropiedadNombre { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime Desde { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Fecha de Fin")]
        [DataType(DataType.Date)]
        public DateTime Hasta { get; set; } = DateTime.Now.AddDays(7);
    }
}