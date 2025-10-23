using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace StayGo.ViewModels.Admin
{
    public class DisponibilidadViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "La propiedad es requerida.")]
        public Guid PropiedadId { get; set; }
        
        [Display(Name = "Habitación (solo para hoteles)")]
        public Guid? HabitacionId { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida.")]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime Desde { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de fin es requerida.")]
        [Display(Name = "Fecha de Fin")]
        [DataType(DataType.Date)]
        public DateTime Hasta { get; set; } = DateTime.Today.AddDays(7);

        // Propiedades para mostrar información
        public string? PropiedadNombre { get; set; }
        public string? HabitacionNombre { get; set; }
        public string? TipoPropiedad { get; set; }
        public bool EsHotel => TipoPropiedad == "Hotel";
        
        // Para los dropdowns
        public IEnumerable<SelectListItem>? Habitaciones { get; set; }
    }
}