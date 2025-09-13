using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums; 
namespace StayGo.Controllers
{
    public class ReservasController : Controller
    {
        private readonly StayGoContext _db;
        public ReservasController(StayGoContext db) => _db = db;

        // GET: /Reservas
        public async Task<IActionResult> Index()
        {
            var reservas = await _db.Reservas
                .Include(r => r.Propiedad)
                .Include(r => r.Habitacion)
                .AsNoTracking()
                .ToListAsync();

            return View(reservas);
        }

        // GET: /Reservas/Crear/{propiedadId}
        [HttpGet]
        public async Task<IActionResult> Crear(Guid propiedadId)
        {
            var prop = await _db.Propiedades
                .Include(p => p.Habitaciones)
                .FirstOrDefaultAsync(p => p.Id == propiedadId);

            if (prop is null) return NotFound();

            var model = new Reserva
            {
                PropiedadId = propiedadId,
                CheckIn = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                CheckOut = DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                Huespedes = 2
            };

            ViewBag.Propiedad = prop;
            return View(model);
        }

        // POST: /Reservas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Reserva model)
        {
            // Revalida reglas de fechas por si vienen manipuladas
            if (model.CheckIn >= model.CheckOut)
                ModelState.AddModelError(nameof(model.CheckIn), "El Check-in debe ser antes del Check-out.");

            if (!ModelState.IsValid)
            {
                ViewBag.Propiedad = await _db.Propiedades
                    .Include(p => p.Habitaciones)
                    .FirstOrDefaultAsync(p => p.Id == model.PropiedadId);
                return View(model);
            }

            // Carga la propiedad y sus habitaciones
            var prop = await _db.Propiedades
                .Include(p => p.Habitaciones)
                .FirstOrDefaultAsync(p => p.Id == model.PropiedadId);

            if (prop is null) return NotFound();

            // Determinar noches
            var noches = model.CheckOut.DayNumber - model.CheckIn.DayNumber;
            if (noches <= 0)
            {
                ModelState.AddModelError("", "El rango de fechas no es válido.");
                ViewBag.Propiedad = prop;
                return View(model);
            }

            // Determinar precio por noche según tipo
            decimal precioNoche;
            if (prop.Tipo == TipoPropiedad.Hotel)
            {
                if (model.HabitacionId is null)
                {
                    ModelState.AddModelError(nameof(model.HabitacionId), "Debes seleccionar una habitación para hoteles.");
                    ViewBag.Propiedad = prop;
                    return View(model);
                }

                var hab = prop.Habitaciones.FirstOrDefault(h => h.Id == model.HabitacionId.Value);
                if (hab is null)
                {
                    ModelState.AddModelError(nameof(model.HabitacionId), "La habitación seleccionada no existe.");
                    ViewBag.Propiedad = prop;
                    return View(model);
                }

                precioNoche = hab.PrecioPorNoche;
            }
            else
            {
                if (!prop.PrecioPorNoche.HasValue || prop.PrecioPorNoche.Value <= 0)
                {
                    ModelState.AddModelError("", "La propiedad no tiene un precio por noche válido.");
                    ViewBag.Propiedad = prop;
                    return View(model);
                }
                precioNoche = prop.PrecioPorNoche.Value;
                model.HabitacionId = null; // consistencia para casas/deptos
            }

            // Chequeo básico de solapamiento (misma propiedad y misma habitación/null)
            var hayConflicto = await _db.Reservas
                .Where(r => r.PropiedadId == model.PropiedadId
                    && r.HabitacionId == model.HabitacionId)
                .AnyAsync(r => !(r.CheckOut <= model.CheckIn || r.CheckIn >= model.CheckOut));

            if (hayConflicto)
            {
                ModelState.AddModelError("", "Las fechas seleccionadas ya están reservadas.");
                ViewBag.Propiedad = prop;
                return View(model);
            }

            // Usuario (por ahora: admin del seed)
            var admin = await _db.Usuarios.FirstAsync(u => u.EsAdmin);
            model.UsuarioId = admin.Id;

            // Calcular total y setear estado
            model.PrecioTotal = precioNoche * noches;
            model.Estado = EstadoReserva.Confirmada;

            _db.Reservas.Add(model);
            await _db.SaveChangesAsync();

            TempData["ok"] = $"Reserva creada por {noches} noche(s). Total: {model.PrecioTotal:C}";
            return RedirectToAction(nameof(Index));
        }
    }
}
