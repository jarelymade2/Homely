using Microsoft.AspNetCore.Mvc.Rendering;
using StayGo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace StayGo.ViewModels.Admin
{
    public class ReservaViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una propiedad.")]
        [Display(Name = "Propiedad")]
        public Guid PropiedadId { get; set; }
        
        [Display(Name = "Habitación")]
        public Guid? HabitacionId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un usuario.")]
        [Display(Name = "Cliente")]
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de Check-In es obligatoria.")]
        [Display(Name = "Fecha de Check-In")]
        [DataType(DataType.Date)]
        public DateTime CheckIn { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La fecha de Check-Out es obligatoria.")]
        [Display(Name = "Fecha de Check-Out")]
        [DataType(DataType.Date)]
        public DateTime CheckOut { get; set; } = DateTime.Now.AddDays(1);

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe haber al menos un huésped.")]
        [Display(Name = "Número de Huéspedes")]
        public int Huespedes { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero.")]
        [Display(Name = "Precio Total")]
        public decimal PrecioTotal { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [Display(Name = "Estado de la Reserva")]
        public EstadoReserva Estado { get; set; }
        
        // Propiedades para mostrar información
        public string? TipoPropiedad { get; set; }
        public bool EsHotel => TipoPropiedad == "Hotel";
        
        // --- Propiedades para los Dropdowns ---
        public IEnumerable<SelectListItem>? Propiedades { get; set; }
        public IEnumerable<SelectListItem>? Usuarios { get; set; }
        public IEnumerable<SelectListItem>? Habitaciones { get; set; }
    }
}