using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.ViewModels.Admin;

namespace StayGo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DisponibilidadController : Controller
    {
        private readonly StayGoContext _db;

        public DisponibilidadController(StayGoContext db)
        {
            _db = db;
        }

        // Muestra el calendario de disponibilidad para una propiedad
        // GET: /Admin/Disponibilidad/Index/{propiedadId}
        [HttpGet]
        public async Task<IActionResult> Index(Guid propiedadId)
        {
            var propiedad = await _db.Propiedades
                .Include(p => p.Disponibilidades)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == propiedadId);

            if (propiedad == null) return NotFound();

            return View(propiedad);
        }


        [HttpGet]
        public async Task<IActionResult> Crear(Guid propiedadId)
        {
            var propiedad = await _db.Propiedades.FindAsync(propiedadId);
            if (propiedad == null) return NotFound();

            var viewModel = new DisponibilidadViewModel
            {
                PropiedadId = propiedad.Id,
                PropiedadNombre = propiedad.Titulo
            };

            return View(viewModel);
        }

        // Procesa el formulario para agregar la disponibilidad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(DisponibilidadViewModel viewModel)
        {
            if (viewModel.Desde >= viewModel.Hasta)
            {
                ModelState.AddModelError("Hasta", "La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            if (!ModelState.IsValid)
            {
                var propiedad = await _db.Propiedades.FindAsync(viewModel.PropiedadId);
                viewModel.PropiedadNombre = propiedad?.Titulo ?? "Propiedad no encontrada";
                return View(viewModel);
            }

            var nuevaDisponibilidad = new Disponibilidad
            {
                Id = Guid.NewGuid(),
                PropiedadId = viewModel.PropiedadId,
                Desde = DateOnly.FromDateTime(viewModel.Desde),
                Hasta = DateOnly.FromDateTime(viewModel.Hasta),
                // HabitacionId se puede agregar aquí si la lógica lo requiere
            };

            _db.Disponibilidades.Add(nuevaDisponibilidad);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Periodo de disponibilidad agregado correctamente.";
            return RedirectToAction(nameof(Index), new { propiedadId = viewModel.PropiedadId });
        }
        
        // Muestra la confirmación para eliminar un periodo
        [HttpGet]
        public async Task<IActionResult> Eliminar(Guid id)
        {
             var disponibilidad = await _db.Disponibilidades
                .Include(d => d.Propiedad)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (disponibilidad == null) return NotFound();

            return View(disponibilidad);
        }

        // Confirma y elimina el periodo de disponibilidad
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(Guid id)
        {
            var disponibilidad = await _db.Disponibilidades.FindAsync(id);
            if (disponibilidad == null) return NotFound();

            var propiedadId = disponibilidad.PropiedadId;
            _db.Disponibilidades.Remove(disponibilidad);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Periodo de disponibilidad eliminado.";
            return RedirectToAction(nameof(Index), new { propiedadId });
        }
        
        // ... (dentro de la clase DisponibilidadController)

        // GET: /Admin/Disponibilidad/GetDisponibilidadesJson?propiedadId=...
        [HttpGet]
        public async Task<IActionResult> GetDisponibilidadesJson(Guid propiedadId)
        {
            var disponibilidades = await _db.Disponibilidades
                .Where(d => d.PropiedadId == propiedadId)
                .Select(d => new 
                {
                    title = "Disponible", // Texto que aparecerá en el calendario
                    start = d.Desde.ToString("yyyy-MM-dd"),
                    // FullCalendar trata la fecha final como exclusiva, sumamos un día para que incluya el último día.
                    end = d.Hasta.AddDays(1).ToString("yyyy-MM-dd"),
                    backgroundColor = "#28a745", // Color verde
                    borderColor = "#28a745"
                })
                .ToListAsync();

            return Json(disponibilidades);
        }
    }
}