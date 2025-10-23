using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums;
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
        [HttpGet]
        public async Task<IActionResult> Index(Guid propiedadId)
        {
            var propiedad = await _db.Propiedades
                .Include(p => p.Disponibilidades)
                .Include(p => p.Habitaciones)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == propiedadId);

            if (propiedad == null) return NotFound();

            ViewBag.Habitaciones = propiedad.Habitaciones
                .Select(h => new SelectListItem 
                { 
                    Value = h.Id.ToString(), 
                    Text = $"{h.Nombre} - Capacidad: {h.Capacidad}" 
                })
                .ToList();

            return View(propiedad);
        }

        [HttpGet]
        public async Task<IActionResult> Crear(Guid propiedadId, Guid? habitacionId = null)
        {
            var propiedad = await _db.Propiedades
                .Include(p => p.Habitaciones)
                .FirstOrDefaultAsync(p => p.Id == propiedadId);
                
            if (propiedad == null) return NotFound();

            var viewModel = new DisponibilidadViewModel
            {
                PropiedadId = propiedad.Id,
                PropiedadNombre = propiedad.Titulo,
                TipoPropiedad = propiedad.Tipo.ToString(),
                HabitacionId = habitacionId,
                Habitaciones = propiedad.Habitaciones
                    .Select(h => new SelectListItem
                    {
                        Value = h.Id.ToString(),
                        Text = $"{h.Nombre} - Capacidad: {h.Capacidad}"
                    }
                    ).ToList()
            };

            // Si se pasa una habitación específica, cargar su nombre
            if (habitacionId.HasValue)
            {
                var habitacion = propiedad.Habitaciones.FirstOrDefault(h => h.Id == habitacionId);
                viewModel.HabitacionNombre = habitacion?.Nombre;
            }

            return View(viewModel);
        }

        // Procesa el formulario para agregar la disponibilidad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(DisponibilidadViewModel viewModel)
        {
            // Validaciones
            if (viewModel.Desde >= viewModel.Hasta)
            {
                ModelState.AddModelError("Hasta", "La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            // Validar que para hoteles se seleccione habitación si se especifica
            var propiedad = await _db.Propiedades
                .Include(p => p.Habitaciones)
                .FirstOrDefaultAsync(p => p.Id == viewModel.PropiedadId);

            if (propiedad != null)
            {
                viewModel.TipoPropiedad = propiedad.Tipo.ToString();
                
                if (propiedad.Tipo == TipoPropiedad.Hotel && viewModel.HabitacionId.HasValue)
                {
                    var habitacion = propiedad.Habitaciones.FirstOrDefault(h => h.Id == viewModel.HabitacionId);
                    if (habitacion == null)
                    {
                        ModelState.AddModelError("HabitacionId", "La habitación seleccionada no existe.");
                    }
                }
                else if (propiedad.Tipo != TipoPropiedad.Hotel && viewModel.HabitacionId.HasValue)
                {
                    // Para no-hoteles, no debería haber habitación seleccionada
                    ModelState.AddModelError("HabitacionId", "Solo los hoteles pueden tener disponibilidad por habitación.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Recargar las habitaciones para el dropdown
                if (propiedad != null)
                {
                    viewModel.Habitaciones = propiedad.Habitaciones
                        .Select(h => new SelectListItem 
                        { 
                            Value = h.Id.ToString(), 
                            Text = $"{h.Nombre} - Capacidad: {h.Capacidad}" 
                        })
                        .ToList();
                }
                return View(viewModel);
            }

            // Verificar que no haya solapamiento
            var existeSolapamiento = await VerificarSolapamiento(
                viewModel.PropiedadId, 
                viewModel.HabitacionId, 
                DateOnly.FromDateTime(viewModel.Desde), 
                DateOnly.FromDateTime(viewModel.Hasta));

            if (existeSolapamiento)
            {
                ModelState.AddModelError("", "Ya existe un período de disponibilidad que se solapa con las fechas seleccionadas.");
                viewModel.Habitaciones = propiedad?.Habitaciones
                    .Select(h => new SelectListItem 
                    { 
                        Value = h.Id.ToString(), 
                        Text = $"{h.Nombre} - Capacidad: {h.Capacidad}" 
                    })
                    .ToList();
                return View(viewModel);
            }

            var nuevaDisponibilidad = new Disponibilidad
            {
                Id = Guid.NewGuid(),
                PropiedadId = viewModel.PropiedadId,
                HabitacionId = viewModel.HabitacionId,
                Desde = DateOnly.FromDateTime(viewModel.Desde),
                Hasta = DateOnly.FromDateTime(viewModel.Hasta)
            };

            _db.Disponibilidades.Add(nuevaDisponibilidad);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Periodo de disponibilidad agregado correctamente.";
            return RedirectToAction(nameof(Index), new { propiedadId = viewModel.PropiedadId });
        }
        
        // Muestra la confirmación para eliminar un periodo
        [HttpGet]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            var disponibilidad = await _db.Disponibilidades
                .Include(d => d.Propiedad)
                .Include(d => d.Habitacion)
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

            TempData["SuccessMessage"] = "Periodo de disponibilidad eliminado.";
            return RedirectToAction(nameof(Index), new { propiedadId });
        }
        
        // Endpoint para el calendario
        [HttpGet]
        public async Task<IActionResult> GetDisponibilidadesJson(Guid propiedadId, Guid? habitacionId = null)
        {
            var query = _db.Disponibilidades
                .Where(d => d.PropiedadId == propiedadId);

            // Filtrar por habitación si se especifica
            if (habitacionId.HasValue)
            {
                query = query.Where(d => d.HabitacionId == habitacionId);
            }
            else
            {
                // Para propiedades no-hotel o vista general, mostrar solo disponibilidad de propiedad completa
                query = query.Where(d => d.HabitacionId == null);
            }

        var disponibilidades = await (from d in query
            join h in _db.Habitaciones on d.HabitacionId equals h.Id into habitacionJoin
            from h in habitacionJoin.DefaultIfEmpty()
            select new 
            {
                title = d.HabitacionId.HasValue ? 
                        $"Disponible - {h.Nombre}" : 
                        "Disponible",
                start = d.Desde.ToString("yyyy-MM-dd"),
                end = d.Hasta.AddDays(1).ToString("yyyy-MM-dd"),
                backgroundColor = d.HabitacionId.HasValue ? "#17a2b8" : "#28a745",
                borderColor = d.HabitacionId.HasValue ? "#17a2b8" : "#28a745",
                extendedProps = new {
                    habitacion = d.HabitacionId.HasValue ? h.Nombre : "Propiedad completa"
                }
            })
            .ToListAsync();
            return Json(disponibilidades);
        }

        // Método privado para verificar solapamiento
        private async Task<bool> VerificarSolapamiento(Guid propiedadId, Guid? habitacionId, DateOnly desde, DateOnly hasta)
        {
            return await _db.Disponibilidades
                .AnyAsync(d => d.PropiedadId == propiedadId &&
                              d.HabitacionId == habitacionId &&
                              d.Desde < hasta && d.Hasta > desde);
        }
    }
}