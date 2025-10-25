using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Integration;
using StayGo.Models;
using StayGo.Models.Enums;

namespace StayGo.Controllers;

[Authorize]
public class PagoController : Controller
{
    private readonly StayGoContext _db;
    private readonly MercadoPagoIntegration _mercadoPago;
    private readonly ILogger<PagoController> _logger;

    public PagoController(
        StayGoContext db, 
        MercadoPagoIntegration mercadoPago,
        ILogger<PagoController> logger)
    {
        _db = db;
        _mercadoPago = mercadoPago;
        _logger = logger;
    }

    /// <summary>
    /// Muestra la página de confirmación con los detalles de la reserva antes de proceder al pago
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Confirmar(Guid propiedadId, Guid? habitacionId, string? checkin, string? checkout, int huespedes = 1)
    {
        _logger.LogInformation("=== CONFIRMAR PAGO ===");
        _logger.LogInformation("PropiedadId recibido: {PropiedadId}", propiedadId);
        _logger.LogInformation("HabitacionId recibido: {HabitacionId}", habitacionId);
        _logger.LogInformation("Check-in: {CheckIn}", checkin);
        _logger.LogInformation("Check-out: {CheckOut}", checkout);
        _logger.LogInformation("Huéspedes: {Huespedes}", huespedes);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Obtener la propiedad
        var propiedad = await _db.Propiedades
            .Include(p => p.Imagenes)
            .Include(p => p.Habitaciones)
            .FirstOrDefaultAsync(p => p.Id == propiedadId);

        _logger.LogInformation("Propiedad encontrada: {Encontrada}", propiedad != null);

        if (propiedad != null)
        {
            _logger.LogInformation("Propiedad: {Titulo}, Precio: {Precio}", propiedad.Titulo, propiedad.PrecioPorNoche);
        }

        if (propiedad == null)
        {
            _logger.LogWarning("Propiedad no encontrada con ID: {PropiedadId}", propiedadId);
            TempData["Error"] = "Propiedad no encontrada.";
            return RedirectToAction("Index", "Propiedad");
        }

        // Si es un hotel, verificar que se haya seleccionado una habitación
        Habitacion? habitacion = null;
        if (propiedad.Tipo == TipoPropiedad.Hotel)
        {
            if (!habitacionId.HasValue)
            {
                TempData["Error"] = "Debes seleccionar una habitación para reservar en un hotel.";
                return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
            }

            habitacion = await _db.Habitaciones
                .FirstOrDefaultAsync(h => h.Id == habitacionId.Value && h.PropiedadId == propiedadId);

            if (habitacion == null)
            {
                TempData["Error"] = "Habitación no encontrada.";
                return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
            }

            _logger.LogInformation("Habitación encontrada: {Nombre}, Precio: {Precio}", habitacion.Nombre, habitacion.PrecioPorNoche);
        }

        // Parsear las fechas
        DateOnly checkInDate = DateOnly.FromDateTime(DateTime.Today);
        DateOnly checkOutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        if (!string.IsNullOrEmpty(checkin) && DateOnly.TryParse(checkin, out var parsedCheckIn))
        {
            checkInDate = parsedCheckIn;
        }

        if (!string.IsNullOrEmpty(checkout) && DateOnly.TryParse(checkout, out var parsedCheckOut))
        {
            checkOutDate = parsedCheckOut;
        }

        // Validar fechas
        if (checkInDate < DateOnly.FromDateTime(DateTime.Today))
        {
            TempData["Error"] = "La fecha de check-in no puede ser anterior a hoy.";
            return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
        }

        if (checkOutDate <= checkInDate)
        {
            TempData["Error"] = "La fecha de check-out debe ser posterior a la fecha de check-in.";
            return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
        }

        // Calcular el número de noches
        int noches = checkOutDate.DayNumber - checkInDate.DayNumber;

        // Calcular el precio total
        decimal precioTotal = 0;
        if (habitacion != null)
        {
            // Si es un hotel, usar el precio de la habitación
            precioTotal = habitacion.PrecioPorNoche * noches;
        }
        else if (propiedad.PrecioPorNoche.HasValue)
        {
            // Si es casa/departamento, usar el precio de la propiedad
            precioTotal = propiedad.PrecioPorNoche.Value * noches;
        }
        else
        {
            TempData["Error"] = "Esta propiedad no tiene precio configurado.";
            return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
        }

        // Pasar los datos a la vista
        ViewBag.Propiedad = propiedad;
        ViewBag.Habitacion = habitacion;
        ViewBag.CheckIn = checkInDate;
        ViewBag.CheckOut = checkOutDate;
        ViewBag.Huespedes = huespedes;
        ViewBag.Noches = noches;
        ViewBag.PrecioTotal = precioTotal;

        return View();
    }

    /// <summary>
    /// Procesa la confirmación y crea la reserva, luego redirige a MercadoPago
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcesarPago(Guid propiedadId, Guid? habitacionId, string checkin, string checkout, int huespedes)
    {
        _logger.LogInformation("=== PROCESAR PAGO ===");
        _logger.LogInformation("PropiedadId: {PropiedadId}", propiedadId);
        _logger.LogInformation("HabitacionId: {HabitacionId}", habitacionId);
        _logger.LogInformation("Check-in: {CheckIn}", checkin);
        _logger.LogInformation("Check-out: {CheckOut}", checkout);
        _logger.LogInformation("Huéspedes: {Huespedes}", huespedes);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Obtener la propiedad
        var propiedad = await _db.Propiedades
            .Include(p => p.Habitaciones)
            .FirstOrDefaultAsync(p => p.Id == propiedadId);

        _logger.LogInformation("Propiedad encontrada: {Encontrada}", propiedad != null);

        if (propiedad != null)
        {
            _logger.LogInformation("Propiedad: {Titulo}, Precio: {Precio}", propiedad.Titulo, propiedad.PrecioPorNoche);
        }

        if (propiedad == null)
        {
            _logger.LogWarning("Propiedad no encontrada con ID: {PropiedadId}", propiedadId);
            TempData["Error"] = "Propiedad no encontrada.";
            return RedirectToAction("Index", "Propiedad");
        }

        // Si es un hotel, verificar que se haya seleccionado una habitación
        Habitacion? habitacion = null;
        if (propiedad.Tipo == TipoPropiedad.Hotel)
        {
            if (!habitacionId.HasValue)
            {
                TempData["Error"] = "Debes seleccionar una habitación para reservar en un hotel.";
                return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
            }

            habitacion = await _db.Habitaciones
                .FirstOrDefaultAsync(h => h.Id == habitacionId.Value && h.PropiedadId == propiedadId);

            if (habitacion == null)
            {
                TempData["Error"] = "Habitación no encontrada.";
                return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
            }

            _logger.LogInformation("Habitación encontrada: {Nombre}, Precio: {Precio}", habitacion.Nombre, habitacion.PrecioPorNoche);
        }

        // Parsear las fechas
        if (!DateOnly.TryParse(checkin, out var checkInDate) || !DateOnly.TryParse(checkout, out var checkOutDate))
        {
            TempData["Error"] = "Fechas inválidas.";
            return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
        }

        // Verificar que el usuario existe
        var usuario = await _db.Users.FindAsync(userId);
        if (usuario == null)
        {
            _logger.LogError("Usuario no encontrado: {UserId}", userId);
            TempData["Error"] = "Usuario no encontrado. Por favor, inicia sesión nuevamente.";
            return RedirectToAction("Login", "Account");
        }

        _logger.LogInformation("Usuario encontrado: {UserId} - {Email}", userId, usuario.Email);

        // Verificar que la propiedad existe en la base de datos
        var propiedadExiste = await _db.Propiedades.AnyAsync(p => p.Id == propiedadId);
        if (!propiedadExiste)
        {
            _logger.LogError("La propiedad {PropiedadId} no existe en la base de datos", propiedadId);
            TempData["Error"] = "La propiedad no existe en la base de datos. Por favor, agrega propiedades primero.";
            return RedirectToAction("Index", "Home");
        }

        // Calcular el precio total
        int noches = checkOutDate.DayNumber - checkInDate.DayNumber;
        decimal precioTotal = 0;
        if (habitacion != null)
        {
            // Si es un hotel, usar el precio de la habitación
            precioTotal = habitacion.PrecioPorNoche * noches;
        }
        else if (propiedad.PrecioPorNoche.HasValue)
        {
            // Si es casa/departamento, usar el precio de la propiedad
            precioTotal = propiedad.PrecioPorNoche.Value * noches;
        }

        _logger.LogInformation("Creando reserva - PropiedadId: {PropiedadId}, HabitacionId: {HabitacionId}, UsuarioId: {UserId}, Noches: {Noches}, Total: {Total}",
            propiedadId, habitacionId, userId, noches, precioTotal);

        // Crear la reserva
        var reserva = new Reserva
        {
            Id = Guid.NewGuid(),
            PropiedadId = propiedadId,
            HabitacionId = habitacionId, // Será null si no es un hotel
            UsuarioId = userId,
            CheckIn = checkInDate,
            CheckOut = checkOutDate,
            Huespedes = huespedes,
            PrecioTotal = precioTotal,
            Estado = EstadoReserva.Pendiente
        };

        try
        {
            _db.Reservas.Add(reserva);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Reserva creada exitosamente: {ReservaId}", reserva.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la reserva");
            TempData["Error"] = "Error al crear la reserva: " + ex.Message;
            return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
        }

        // Redirigir a iniciar el pago
        return RedirectToAction("Iniciar", new { reservaId = reserva.Id });
    }

    /// <summary>
    /// Inicia el proceso de pago para una reserva
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Iniciar(Guid reservaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Obtener la reserva con la propiedad
        var reserva = await _db.Reservas
            .Include(r => r.Propiedad)
            .FirstOrDefaultAsync(r => r.Id == reservaId && r.UsuarioId == userId);

        if (reserva == null)
        {
            TempData["Error"] = "Reserva no encontrada o no tienes permiso para acceder a ella.";
            return RedirectToAction("History", "Reserva");
        }

        // Verificar que la reserva esté en estado Pendiente
        if (reserva.Estado != EstadoReserva.Pendiente)
        {
            TempData["Error"] = "Esta reserva ya no está disponible para pago.";
            return RedirectToAction("History", "Reserva");
        }

        // Verificar si ya existe un pago exitoso para esta reserva
        var pagoExistente = await _db.Pagos
            .FirstOrDefaultAsync(p => p.ReservaId == reservaId && p.Estado == EstadoPago.Exitoso);

        if (pagoExistente != null)
        {
            TempData["Error"] = "Esta reserva ya ha sido pagada.";
            return RedirectToAction("History", "Reserva");
        }

        // Crear URLs de retorno
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var urlSuccess = $"{baseUrl}/Pago/Exito?reservaId={reservaId}";
        var urlFailure = $"{baseUrl}/Pago/Fallo?reservaId={reservaId}";
        var urlPending = $"{baseUrl}/Pago/Pendiente?reservaId={reservaId}";

        // Crear preferencia de pago en MercadoPago
        var titulo = $"Reserva en {reserva.Propiedad?.Titulo ?? "StayGo"}";
        var descripcion = $"Check-in: {reserva.CheckIn:yyyy-MM-dd}, Check-out: {reserva.CheckOut:yyyy-MM-dd}, Huéspedes: {reserva.Huespedes}";

        _logger.LogInformation("=== INICIANDO PAGO ===");
        _logger.LogInformation("ReservaId: {ReservaId}", reservaId);
        _logger.LogInformation("Título: {Titulo}", titulo);
        _logger.LogInformation("Monto: {Monto}", reserva.PrecioTotal);
        _logger.LogInformation("URL Success: {UrlSuccess}", urlSuccess);
        _logger.LogInformation("URL Failure: {UrlFailure}", urlFailure);
        _logger.LogInformation("URL Pending: {UrlPending}", urlPending);

        var urlPago = await _mercadoPago.CrearPreferenciaPagoAsync(
            reservaId,
            titulo,
            descripcion,
            reserva.PrecioTotal,
            urlSuccess,
            urlFailure,
            urlPending
        );

        _logger.LogInformation("URL de pago recibida: {UrlPago}", urlPago ?? "NULL");

        if (string.IsNullOrEmpty(urlPago))
        {
            _logger.LogError("No se pudo crear la preferencia de pago en MercadoPago");
            TempData["Error"] = "No se pudo iniciar el proceso de pago. Por favor, intenta más tarde.";
            return RedirectToAction("History", "Reserva");
        }

        // Crear registro de pago en estado Pendiente
        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            ReservaId = reservaId,
            Monto = reserva.PrecioTotal,
            Moneda = "PEN",
            Metodo = MetodoPago.Tarjeta, // MercadoPago soporta múltiples métodos
            Estado = EstadoPago.Pendiente,
            CreadoEn = DateTime.UtcNow
        };

        _db.Pagos.Add(pago);
        await _db.SaveChangesAsync();

        _logger.LogInformation($"Redirigiendo a MercadoPago para reserva {reservaId}");

        // Redirigir a MercadoPago
        return Redirect(urlPago);
    }

    /// <summary>
    /// Página de éxito después del pago
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Exito(Guid reservaId, string? payment_id, string? status, string? external_reference)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var reserva = await _db.Reservas
            .Include(r => r.Propiedad)
            .FirstOrDefaultAsync(r => r.Id == reservaId && r.UsuarioId == userId);

        if (reserva == null)
        {
            return NotFound();
        }

        // Actualizar el pago
        var pago = await _db.Pagos
            .Where(p => p.ReservaId == reservaId)
            .OrderByDescending(p => p.CreadoEn)
            .FirstOrDefaultAsync();

        if (pago != null)
        {
            pago.Estado = EstadoPago.Exitoso;
            pago.TransaccionRef = payment_id;
            pago.ActualizadoEn = DateTime.UtcNow;

            // Actualizar estado de la reserva a Confirmada
            reserva.Estado = EstadoReserva.Confirmada;

            await _db.SaveChangesAsync();

            _logger.LogInformation($"Pago exitoso para reserva {reservaId}, payment_id: {payment_id}");
        }

        ViewBag.Reserva = reserva;
        ViewBag.PaymentId = payment_id;
        
        return View();
    }

    /// <summary>
    /// Página de fallo después del pago
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Fallo(Guid reservaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var reserva = await _db.Reservas
            .Include(r => r.Propiedad)
            .FirstOrDefaultAsync(r => r.Id == reservaId && r.UsuarioId == userId);

        if (reserva == null)
        {
            return NotFound();
        }

        // Actualizar el pago
        var pago = await _db.Pagos
            .Where(p => p.ReservaId == reservaId)
            .OrderByDescending(p => p.CreadoEn)
            .FirstOrDefaultAsync();

        if (pago != null)
        {
            pago.Estado = EstadoPago.Fallido;
            pago.ActualizadoEn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogWarning($"Pago fallido para reserva {reservaId}");
        }

        ViewBag.Reserva = reserva;
        
        return View();
    }

    /// <summary>
    /// Página de pago pendiente
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Pendiente(Guid reservaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var reserva = await _db.Reservas
            .Include(r => r.Propiedad)
            .FirstOrDefaultAsync(r => r.Id == reservaId && r.UsuarioId == userId);

        if (reserva == null)
        {
            return NotFound();
        }

        ViewBag.Reserva = reserva;
        
        return View();
    }
}

