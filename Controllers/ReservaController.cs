using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums;

namespace StayGo.Controllers
{
    public class ReservaController : Controller
    {
        private readonly StayGoContext _db;
        private readonly ILogger<ReservaController> _logger;

        public ReservaController(StayGoContext db, ILogger<ReservaController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: /Reserva/Create - Redirigir de vuelta si alguien intenta acceder por GET
        [HttpGet]
        public IActionResult Create(Guid propiedadId)
        {
            _logger.LogWarning($"Intento de acceso GET a /Reserva/Create con propiedadId: {propiedadId}");
            return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
        }

        // POST: /Reserva/Create - Crear reserva directamente y redirigir al pago
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid propiedadId, string? checkin, string? checkout, int huespedes = 1)
        {
            _logger.LogInformation($"=== CREANDO RESERVA ===");
            _logger.LogInformation($"PropiedadId: {propiedadId}");
            _logger.LogInformation($"Check-in: {checkin}");
            _logger.LogInformation($"Check-out: {checkout}");
            _logger.LogInformation($"Huéspedes: {huespedes}");

            // Parsear las fechas
            DateOnly checkInDate = DateOnly.FromDateTime(DateTime.Today);
            DateOnly checkOutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            var culture = CultureInfo.InvariantCulture;

            if (!string.IsNullOrWhiteSpace(checkin) && DateOnly.TryParseExact(checkin, "yyyy-MM-dd", culture, DateTimeStyles.None, out var cIn))
            {
                checkInDate = cIn;
            }

            if (!string.IsNullOrWhiteSpace(checkout) && DateOnly.TryParseExact(checkout, "yyyy-MM-dd", culture, DateTimeStyles.None, out var cOut))
            {
                checkOutDate = cOut;
            }

            // Crear el objeto de reserva
            var input = new Reserva
            {
                PropiedadId = propiedadId,
                CheckIn = checkInDate,
                CheckOut = checkOutDate,
                Huespedes = huespedes
            };
            // Re-obtener propiedad para cálculo del precio y validaciones
            var prop = await _db.Propiedades.FirstOrDefaultAsync(p => p.Id == input.PropiedadId);
            if (prop == null) return NotFound();

            // Validaciones básicas
            if (input.CheckIn >= input.CheckOut)
            {
                ModelState.AddModelError(string.Empty, "La fecha de salida debe ser posterior a la de entrada.");
            }

            if (input.Huespedes <= 0)
            {
                ModelState.AddModelError(nameof(input.Huespedes), "Debe indicar al menos 1 huésped.");
            }

            // Comprobar disponibilidad
            if (!await EsDisponible(input.PropiedadId, input.CheckIn, input.CheckOut))
            {
                ModelState.AddModelError(string.Empty, "Las fechas seleccionadas no están disponibles para esa propiedad.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Error en la reserva: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction("Details", "Propiedad", new { id = input.PropiedadId });
            }

            // Calcular precio total: noches * precioPorNoche (si el precio es null, 0)
            var noches = (input.CheckOut.ToDateTime(TimeOnly.MinValue) - input.CheckIn.ToDateTime(TimeOnly.MinValue)).Days;
            var precioPorNoche = prop.PrecioPorNoche ?? 0m;
            input.PrecioTotal = precioPorNoche * Math.Max(0, noches);

            // Asignar usuario (usar ClaimTypes.NameIdentifier)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // fallback: nombre
                userId = User.Identity?.Name ?? "anonymous";
            }
            input.UsuarioId = userId;

            // Estado inicial: Pendiente (según tu enum)
            input.Id = Guid.NewGuid();
            input.Estado = EstadoReserva.Pendiente;

            _db.Reservas.Add(input);
            await _db.SaveChangesAsync();

            TempData["MensajeReserva"] = "Reserva creada correctamente. Procede con el pago para confirmarla.";

            // Redirigir al proceso de pago
            return RedirectToAction("Iniciar", "Pago", new { reservaId = input.Id });
        }

        // GET: /Reserva/History
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // si falla (muy raro porque [Authorize] obliga autenticación), devolvemos vacío
                return View(new List<Reserva>());
            }

            var reservas = await _db.Reservas
                .Include(r => r.Propiedad)
                .Include(r => r.Pagos) // aunque dejemos pagos aparte, no molesta incluir
                .Where(r => r.UsuarioId == userId)
                .ToListAsync();

            // Ordenar en memoria después de traer los datos
            reservas = reservas.OrderByDescending(r => r.CheckIn).ToList();

            return View(reservas);
        }

        // Método de disponibilidad compatible con DateOnly
        private async Task<bool> EsDisponible(Guid propiedadId, DateOnly inicio, DateOnly fin)
        {
            // Obtenemos reservas existentes (confirmadas / pendientes) que puedan solapar
            var existentes = await _db.Reservas
                .Where(r => r.PropiedadId == propiedadId && r.Estado != EstadoReserva.Cancelada)
                .ToListAsync();

            foreach (var r in existentes)
            {
                // Convertir a DateTime para comparar (comparación: inicio < r.fin && fin > r.inicio)
                var aInicio = inicio.ToDateTime(TimeOnly.MinValue);
                var aFin = fin.ToDateTime(TimeOnly.MinValue);
                var bInicio = r.CheckIn.ToDateTime(TimeOnly.MinValue);
                var bFin = r.CheckOut.ToDateTime(TimeOnly.MinValue);

                if (aInicio < bFin && aFin > bInicio)
                {
                    return false; // hay solapamiento
                }
            }

            return true;
        }
    }
}
