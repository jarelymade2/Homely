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

            _logger.LogInformation("History - UserId obtenido: {UserId}", userId);

            var reservas = await _db.Reservas
                .Include(r => r.Propiedad)
                .Include(r => r.Pagos) // aunque dejemos pagos aparte, no molesta incluir
                .Where(r => r.UsuarioId == userId)
                .ToListAsync();

            // Log de debug
            var todasLasReservas = await _db.Reservas.ToListAsync();
            _logger.LogInformation("Total de reservas en BD: {Total}", todasLasReservas.Count);
            foreach (var r in todasLasReservas.Take(5))
            {
                _logger.LogInformation("Reserva ID: {Id}, UsuarioId: {UsuarioId}", r.Id, r.UsuarioId);
            }

            // Ordenar en memoria después de traer los datos
            reservas = reservas.OrderByDescending(r => r.CheckIn).ToList();

            return View(reservas);
        }

        /// <summary>
        /// Elimina una reserva específica (solo si pertenece al usuario autenticado)
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Obtener la reserva primero
                var reserva = await _db.Reservas.FirstOrDefaultAsync(r => r.Id == id);

                if (reserva == null)
                {
                    TempData["Error"] = "Reserva no encontrada.";
                    return RedirectToAction("History");
                }

                // Verificar que pertenece al usuario autenticado
                if (reserva.UsuarioId != userId)
                {
                    _logger.LogWarning($"Intento de eliminar reserva {id} por usuario no autorizado. UsuarioId esperado: {reserva.UsuarioId}, UsuarioId actual: {userId}");
                    TempData["Error"] = "No tienes permiso para eliminar esta reserva.";
                    return RedirectToAction("History");
                }

                // Solo permitir eliminar reservas pendientes o canceladas
                if (reserva.Estado != EstadoReserva.Pendiente && reserva.Estado != EstadoReserva.Cancelada)
                {
                    TempData["Error"] = "Solo puedes eliminar reservas pendientes o canceladas.";
                    return RedirectToAction("History");
                }

                // Eliminar pagos asociados primero
                var pagos = await _db.Pagos.Where(p => p.ReservaId == id).ToListAsync();
                _db.Pagos.RemoveRange(pagos);

                // Eliminar la reserva
                _db.Reservas.Remove(reserva);
                await _db.SaveChangesAsync();

                TempData["MensajeReserva"] = "Reserva eliminada correctamente.";
                _logger.LogInformation("Reserva {ReservaId} eliminada por usuario {UserId}", id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar reserva {ReservaId}", id);
                TempData["Error"] = "Error al eliminar la reserva: " + ex.Message;
            }

            return RedirectToAction("History");
        }

        /// <summary>
        /// Elimina múltiples reservas seleccionadas (AJAX)
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<Guid> reservaIds)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                if (reservaIds == null || reservaIds.Count == 0)
                {
                    return Json(new { success = false, message = "No hay reservas seleccionadas." });
                }

                // Obtener todas las reservas solicitadas
                var reservas = await _db.Reservas
                    .Where(r => reservaIds.Contains(r.Id))
                    .ToListAsync();

                if (reservas.Count == 0)
                {
                    return Json(new { success = false, message = "No se encontraron reservas para eliminar." });
                }

                // Validar que todas pertenezcan al usuario autenticado
                var noAutorizadas = reservas.Where(r => r.UsuarioId != userId).ToList();
                if (noAutorizadas.Count > 0)
                {
                    _logger.LogWarning("Intento de eliminar {Count} reservas no autorizadas por usuario {UserId}", noAutorizadas.Count, userId);
                    return Json(new { success = false, message = "No tienes permiso para eliminar algunas de estas reservas." });
                }

                // Validar que todas sean eliminables (pendientes o canceladas)
                var noEliminables = reservas.Where(r => r.Estado != EstadoReserva.Pendiente && r.Estado != EstadoReserva.Cancelada).ToList();
                if (noEliminables.Count > 0)
                {
                    return Json(new { success = false, message = $"No puedes eliminar {noEliminables.Count} reserva(s) confirmada(s) o finalizada(s)." });
                }

                // Eliminar pagos asociados
                var pagoIds = reservas.Select(r => r.Id).ToList();
                var pagos = await _db.Pagos.Where(p => pagoIds.Contains(p.ReservaId)).ToListAsync();
                _db.Pagos.RemoveRange(pagos);

                // Eliminar las reservas
                _db.Reservas.RemoveRange(reservas);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Se eliminaron {Count} reservas del usuario {UserId}", reservas.Count, userId);

                return Json(new { success = true, message = $"Se eliminaron {reservas.Count} reserva(s) correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar múltiples reservas");
                return Json(new { success = false, message = "Error al eliminar las reservas: " + ex.Message });
            }
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
