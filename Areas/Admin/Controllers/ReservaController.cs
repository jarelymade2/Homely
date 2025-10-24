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
    public class ReservaController : Controller
    {
        private readonly StayGoContext _context;

        public ReservaController(StayGoContext context)
        {
            _context = context;
        }

        // GET: Admin/Reserva
        public async Task<IActionResult> Index()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Propiedad)
                .Include(r => r.Habitacion)
                .Include(r => r.Usuario)
                .OrderByDescending(r => r.CheckIn)
                .ToListAsync();
            return View(reservas);
        }

        // GET: Admin/Reserva/Create
        public async Task<IActionResult> Crear()
        {
            var viewModel = new ReservaViewModel
            {
                Propiedades = await GetPropiedadesSelectList(),
                Usuarios = await GetUsuariosSelectList(),
                Habitaciones = new List<SelectListItem>()
            };
            return View(viewModel);
        }

        // POST: Admin/Reserva/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ReservaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Validaciones básicas de fechas
                    var erroresFechas = ValidarFechas(viewModel.CheckIn, viewModel.CheckOut);
                    if (!string.IsNullOrEmpty(erroresFechas))
                    {
                        ModelState.AddModelError("", erroresFechas);
                    }
                    else
                    {
                        // 2. Obtener y validar propiedad
                        var propiedad = await _context.Propiedades
                            .Include(p => p.Habitaciones)
                            .FirstOrDefaultAsync(p => p.Id == viewModel.PropiedadId);

                        if (propiedad == null)
                        {
                            ModelState.AddModelError("PropiedadId", "La propiedad seleccionada no existe.");
                        }
                        else
                        {
                            // 3. Validar tipo de propiedad vs habitación
                            if (propiedad.Tipo == TipoPropiedad.Hotel && !viewModel.HabitacionId.HasValue)
                            {
                                ModelState.AddModelError("HabitacionId", "Para hoteles debe seleccionar una habitación.");
                            }
                            else if (propiedad.Tipo != TipoPropiedad.Hotel && viewModel.HabitacionId.HasValue)
                            {
                                ModelState.AddModelError("HabitacionId", "Solo los hoteles requieren selección de habitación.");
                            }
                            else
                            {
                                // 4. Validar capacidad
                                var errorCapacidad = await ValidarCapacidad(viewModel, propiedad);
                                if (!string.IsNullOrEmpty(errorCapacidad))
                                {
                                    ModelState.AddModelError("Huespedes", errorCapacidad);
                                }
                                else
                                {
                                    // 5. Validar disponibilidad
                                    var errorDisponibilidad = await ValidarDisponibilidad(viewModel, propiedad);
                                    if (!string.IsNullOrEmpty(errorDisponibilidad))
                                    {
                                        ModelState.AddModelError("", errorDisponibilidad);
                                    }
                                    else
                                    {
                                        // 6. Validar y calcular precio
                                        var errorPrecio = ValidarPrecio(viewModel.PrecioTotal);
                                        if (!string.IsNullOrEmpty(errorPrecio))
                                        {
                                            ModelState.AddModelError("PrecioTotal", errorPrecio);
                                        }
                                        else
                                        {
                                            // 7. Si el precio es 0, calcularlo automáticamente
                                            if (viewModel.PrecioTotal <= 0)
                                            {
                                                viewModel.PrecioTotal = await CalcularPrecioTotal(viewModel, propiedad);
                                            }

                                            // 8. Crear la reserva
                                            var reserva = new Reserva
                                            {
                                                Id = Guid.NewGuid(),
                                                PropiedadId = viewModel.PropiedadId,
                                                HabitacionId = viewModel.HabitacionId,
                                                UsuarioId = viewModel.UsuarioId,
                                                CheckIn = DateOnly.FromDateTime(viewModel.CheckIn),
                                                CheckOut = DateOnly.FromDateTime(viewModel.CheckOut),
                                                Huespedes = viewModel.Huespedes,
                                                PrecioTotal = viewModel.PrecioTotal,
                                                Estado = viewModel.Estado,
                                                
                                            };

                                            _context.Add(reserva);
                                            await _context.SaveChangesAsync();

                                            TempData["SuccessMessage"] = "Reserva creada exitosamente.";
                                            return RedirectToAction(nameof(Index));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Error de base de datos al crear la reserva.");
                    // Log the exception
                    System.Diagnostics.Debug.WriteLine($"DbUpdateException: {ex.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                }
            }

            await CargarSelectLists(viewModel);
            return View(viewModel);
        }

        // GET: Admin/Reserva/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();
            
            var reserva = await _context.Reservas
                .Include(r => r.Propiedad)
                .Include(r => r.Habitacion)
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (reserva == null) return NotFound();

            var viewModel = new ReservaViewModel
            {
                Id = reserva.Id,
                PropiedadId = reserva.PropiedadId,
                HabitacionId = reserva.HabitacionId,
                UsuarioId = reserva.UsuarioId,
                CheckIn = reserva.CheckIn.ToDateTime(TimeOnly.MinValue),
                CheckOut = reserva.CheckOut.ToDateTime(TimeOnly.MinValue),
                Huespedes = reserva.Huespedes,
                PrecioTotal = reserva.PrecioTotal,
                Estado = reserva.Estado,
                TipoPropiedad = reserva.Propiedad?.Tipo.ToString()
            };
            
            await CargarSelectLists(viewModel);
            return View(viewModel);
        }

        // POST: Admin/Reserva/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ReservaViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Validaciones básicas de fechas
                    var erroresFechas = ValidarFechas(viewModel.CheckIn, viewModel.CheckOut);
                    if (!string.IsNullOrEmpty(erroresFechas))
                    {
                        ModelState.AddModelError("", erroresFechas);
                    }
                    else
                    {
                        // 2. Obtener y validar propiedad
                        var propiedad = await _context.Propiedades
                            .FirstOrDefaultAsync(p => p.Id == viewModel.PropiedadId);

                        if (propiedad == null)
                        {
                            ModelState.AddModelError("PropiedadId", "La propiedad seleccionada no existe.");
                        }
                        else
                        {
                            // 3. Validar tipo de propiedad vs habitación
                            if (propiedad.Tipo == TipoPropiedad.Hotel && !viewModel.HabitacionId.HasValue)
                            {
                                ModelState.AddModelError("HabitacionId", "Para hoteles debe seleccionar una habitación.");
                            }
                            else if (propiedad.Tipo != TipoPropiedad.Hotel && viewModel.HabitacionId.HasValue)
                            {
                                ModelState.AddModelError("HabitacionId", "Solo los hoteles requieren selección de habitación.");
                            }
                            else
                            {
                                // 4. Validar capacidad
                                var errorCapacidad = await ValidarCapacidad(viewModel, propiedad);
                                if (!string.IsNullOrEmpty(errorCapacidad))
                                {
                                    ModelState.AddModelError("Huespedes", errorCapacidad);
                                }
                                else
                                {
                                    // 5. Validar disponibilidad (excluyendo la reserva actual)
                                    var errorDisponibilidad = await ValidarDisponibilidad(viewModel, propiedad, id);
                                    if (!string.IsNullOrEmpty(errorDisponibilidad))
                                    {
                                        ModelState.AddModelError("", errorDisponibilidad);
                                    }
                                    else
                                    {
                                        // 6. Validar precio
                                        var errorPrecio = ValidarPrecio(viewModel.PrecioTotal);
                                        if (!string.IsNullOrEmpty(errorPrecio))
                                        {
                                            ModelState.AddModelError("PrecioTotal", errorPrecio);
                                        }
                                        else
                                        {
                                            // 7. Actualizar la reserva
                                            var reservaToUpdate = await _context.Reservas.FindAsync(id);
                                            if (reservaToUpdate == null) return NotFound();
                                            
                                            reservaToUpdate.PropiedadId = viewModel.PropiedadId;
                                            reservaToUpdate.HabitacionId = viewModel.HabitacionId;
                                            reservaToUpdate.UsuarioId = viewModel.UsuarioId;
                                            reservaToUpdate.CheckIn = DateOnly.FromDateTime(viewModel.CheckIn);
                                            reservaToUpdate.CheckOut = DateOnly.FromDateTime(viewModel.CheckOut);
                                            reservaToUpdate.Huespedes = viewModel.Huespedes;
                                            reservaToUpdate.PrecioTotal = viewModel.PrecioTotal;
                                            reservaToUpdate.Estado = viewModel.Estado;

                                            await _context.SaveChangesAsync();

                                            TempData["SuccessMessage"] = "Reserva actualizada exitosamente.";
                                            return RedirectToAction(nameof(Index));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Error de base de datos al actualizar la reserva.");
                    System.Diagnostics.Debug.WriteLine($"DbUpdateException: {ex.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                }
            }
            
            await CargarSelectLists(viewModel);
            return View(viewModel);
        }

        // AJAX: Obtener habitaciones por propiedad
        [HttpGet]
        public async Task<JsonResult> GetHabitacionesByPropiedad(Guid propiedadId)
        {
            try
            {
                var propiedad = await _context.Propiedades
                    .Include(p => p.Habitaciones)
                    .FirstOrDefaultAsync(p => p.Id == propiedadId);
                    
                if (propiedad == null)
                    return Json(new { success = false, message = "Propiedad no encontrada" });

                var habitaciones = propiedad.Habitaciones
                    .Select(h => new { 
                        id = h.Id, 
                        text = $"{h.Nombre} - Capacidad: {h.Capacidad} - ${h.PrecioPorNoche}/noche",
                        capacidad = h.Capacidad,
                        precio = h.PrecioPorNoche
                    })
                    .ToList();

                return Json(new { 
                    success = true, 
                    habitaciones, 
                    esHotel = propiedad.Tipo == TipoPropiedad.Hotel,
                    capacidadPropiedad = propiedad.Capacidad,
                    precioPropiedad = propiedad.PrecioPorNoche
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // AJAX: Validar disponibilidad en tiempo real
        [HttpGet]
        public async Task<JsonResult> ValidarDisponibilidad(Guid propiedadId, Guid? habitacionId, DateTime checkIn, DateTime checkOut, Guid? reservaId = null)
        {
            try
            {
                // 1. Validar fechas primero
                var erroresFechas = ValidarFechas(checkIn, checkOut);
                if (!string.IsNullOrEmpty(erroresFechas))
                    return Json(new { success = false, message = erroresFechas });

                var propiedad = await _context.Propiedades
                    .Include(p => p.Habitaciones)
                    .FirstOrDefaultAsync(p => p.Id == propiedadId);

                if (propiedad == null)
                    return Json(new { success = false, message = "Propiedad no encontrada" });

                var checkInDate = DateOnly.FromDateTime(checkIn);
                var checkOutDate = DateOnly.FromDateTime(checkOut);

                // 2. Validar disponibilidad
                bool disponible;
                string mensajeDisponibilidad;

                if (propiedad.Tipo == TipoPropiedad.Hotel && habitacionId.HasValue)
                {
                    // Validar habitación específica
                    disponible = !await _context.Reservas
                        .AnyAsync(r => r.HabitacionId == habitacionId &&
                                      r.Id != reservaId &&
                                      r.Estado != EstadoReserva.Cancelada &&
                                      r.CheckIn < checkOutDate && r.CheckOut > checkInDate);
                    
                    mensajeDisponibilidad = disponible ? "Habitación disponible" : "Habitación no disponible en las fechas seleccionadas";
                }
                else
                {
                    // Validar propiedad completa
                    disponible = !await _context.Reservas
                        .AnyAsync(r => r.PropiedadId == propiedadId &&
                                      r.HabitacionId == null &&
                                      r.Id != reservaId &&
                                      r.Estado != EstadoReserva.Cancelada &&
                                      r.CheckIn < checkOutDate && r.CheckOut > checkInDate);
                    
                    mensajeDisponibilidad = disponible ? "Propiedad disponible" : "Propiedad no disponible en las fechas seleccionadas";
                }

                // 3. Calcular precio sugerido
                decimal precioSugerido = 0;
                int noches = (checkOutDate.DayNumber - checkInDate.DayNumber);

                if (propiedad.Tipo == TipoPropiedad.Hotel && habitacionId.HasValue)
                {
                    var habitacion = propiedad.Habitaciones.FirstOrDefault(h => h.Id == habitacionId);
                    precioSugerido = habitacion?.PrecioPorNoche * noches ?? 0;
                }
                else
                {
                    precioSugerido = propiedad.PrecioPorNoche.GetValueOrDefault() * noches;
                }

                return Json(new { 
                    success = true, 
                    disponible,
                    mensaje = mensajeDisponibilidad,
                    precioSugerido,
                    noches
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: Admin/Reserva/Cancel/5
        public async Task<IActionResult> Cancel(Guid? id)
        {
            if (id == null) return NotFound();
            var reserva = await _context.Reservas
                .Include(r => r.Propiedad)
                .Include(r => r.Habitacion)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reserva == null) return NotFound();
            return View(reserva);
        }

        // POST: Admin/Reserva/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(Guid id)
        {
            try
            {
                var reserva = await _context.Reservas.FindAsync(id);
                if (reserva != null)
                {
                    reserva.Estado = EstadoReserva.Cancelada;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Reserva cancelada exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Reserva no encontrada.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cancelar la reserva: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- MÉTODOS DE VALIDACIÓN PRIVADOS ---

        private string ValidarFechas(DateTime checkIn, DateTime checkOut)
        {
            if (checkIn < DateTime.Today)
                return "La fecha de check-in no puede ser en el pasado.";

            if (checkOut <= checkIn)
                return "La fecha de check-out debe ser posterior al check-in.";

            var diferenciaDias = (checkOut - checkIn).TotalDays;
            if (diferenciaDias > 90)
                return "La estadía no puede exceder los 90 días.";

            if (diferenciaDias < 1)
                return "La estadía mínima es de 1 noche.";

            return string.Empty;
        }

        private async Task<string> ValidarCapacidad(ReservaViewModel viewModel, Propiedad propiedad)
        {
            if (viewModel.Huespedes <= 0)
                return "Debe haber al menos un huésped.";

            int capacidadMaxima;

            if (propiedad.Tipo == TipoPropiedad.Hotel && viewModel.HabitacionId.HasValue)
            {
                var habitacion = await _context.Habitaciones
                    .FirstOrDefaultAsync(h => h.Id == viewModel.HabitacionId);
                
                if (habitacion == null)
                    return "La habitación seleccionada no existe.";

                capacidadMaxima = habitacion.Capacidad;

                if (viewModel.Huespedes > capacidadMaxima)
                    return $"La capacidad máxima de la habitación '{habitacion.Nombre}' es de {capacidadMaxima} huéspedes.";
            }
            else
            {
                capacidadMaxima = propiedad.Capacidad ?? 0;
                if (capacidadMaxima == 0)
                    return "La propiedad no tiene capacidad definida.";

                if (viewModel.Huespedes > capacidadMaxima)
                    return $"La capacidad máxima de la propiedad es de {capacidadMaxima} huéspedes.";
            }

            return string.Empty;
        }

        private async Task<string> ValidarDisponibilidad(ReservaViewModel viewModel, Propiedad propiedad, Guid? reservaIdExcluir = null)
        {
            var checkInDate = DateOnly.FromDateTime(viewModel.CheckIn);
            var checkOutDate = DateOnly.FromDateTime(viewModel.CheckOut);

            bool existeSolapamiento;

            if (propiedad.Tipo == TipoPropiedad.Hotel && viewModel.HabitacionId.HasValue)
            {
                // Validar solapamiento para habitación específica
                existeSolapamiento = await _context.Reservas
                    .AnyAsync(r => r.HabitacionId == viewModel.HabitacionId &&
                                  r.Id != reservaIdExcluir &&
                                  r.Estado != EstadoReserva.Cancelada &&
                                  r.CheckIn < checkOutDate && r.CheckOut > checkInDate);
                
                if (existeSolapamiento)
                    return "La habitación seleccionada no está disponible para las fechas indicadas. Ya existe una reserva activa en ese período.";
            }
            else
            {
                // Validar solapamiento para propiedad completa
                existeSolapamiento = await _context.Reservas
                    .AnyAsync(r => r.PropiedadId == viewModel.PropiedadId &&
                                  r.HabitacionId == null &&
                                  r.Id != reservaIdExcluir &&
                                  r.Estado != EstadoReserva.Cancelada &&
                                  r.CheckIn < checkOutDate && r.CheckOut > checkInDate);
                
                if (existeSolapamiento)
                    return "La propiedad no está disponible para las fechas indicadas. Ya existe una reserva activa en ese período.";
            }

            return string.Empty;
        }

        private string ValidarPrecio(decimal precioTotal)
        {
            if (precioTotal < 0)
                return "El precio no puede ser negativo.";

            if (precioTotal == 0)
                return "El precio debe ser mayor a cero.";

            if (precioTotal > 1000000) // Límite razonable
                return "El precio excede el límite máximo permitido.";

            return string.Empty;
        }

        private async Task<decimal> CalcularPrecioTotal(ReservaViewModel viewModel, Propiedad propiedad)
        {
            var noches = (DateOnly.FromDateTime(viewModel.CheckOut).DayNumber - 
                         DateOnly.FromDateTime(viewModel.CheckIn).DayNumber);

            if (propiedad.Tipo == TipoPropiedad.Hotel && viewModel.HabitacionId.HasValue)
            {
                var habitacion = await _context.Habitaciones
                    .FirstOrDefaultAsync(h => h.Id == viewModel.HabitacionId);
                
                return (habitacion?.PrecioPorNoche ?? 0) * noches;
            }
            else
            {
                return (propiedad.PrecioPorNoche ?? 0) * noches;
            }
        }

        // --- MÉTODOS AUXILIARES ---
        private async Task CargarSelectLists(ReservaViewModel viewModel)
        {
            viewModel.Propiedades = await GetPropiedadesSelectList();
            viewModel.Usuarios = await GetUsuariosSelectList();
            
            if (viewModel.PropiedadId != Guid.Empty)
            {
                viewModel.Habitaciones = await GetHabitacionesSelectList(viewModel.PropiedadId);
                var propiedad = await _context.Propiedades
                    .FirstOrDefaultAsync(p => p.Id == viewModel.PropiedadId);
                viewModel.TipoPropiedad = propiedad?.Tipo.ToString();
            }
            else
            {
                viewModel.Habitaciones = new List<SelectListItem>();
            }
        }
        
        private async Task<IEnumerable<SelectListItem>> GetPropiedadesSelectList()
        {
            return await _context.Propiedades
                .Where(p => p.Capacidad > 0 && 
                           (p.Tipo != TipoPropiedad.Hotel || p.Habitaciones.Any()))
                .OrderBy(p => p.Titulo)
                .Select(p => new SelectListItem { 
                    Value = p.Id.ToString(), 
                    Text = $"{p.Titulo} ({p.Tipo}) - Capacidad: {p.Capacidad} - ${p.PrecioPorNoche}/noche" 
                })
                .ToListAsync();
        }
    
        private async Task<IEnumerable<SelectListItem>> GetUsuariosSelectList()
        {
            return await _context.Users
                .OrderBy(u => u.Email)
                .Select(u => new SelectListItem { Value = u.Id, Text = u.Email })
                .ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetHabitacionesSelectList(Guid propiedadId)
        {
            return await _context.Habitaciones
                .Where(h => h.PropiedadId == propiedadId)
                .OrderBy(h => h.Nombre)
                .Select(h => new SelectListItem
                {
                    Value = h.Id.ToString(),
                    Text = $"{h.Nombre} - Capacidad: {h.Capacidad} - ${h.PrecioPorNoche}/noche"
                })
                .ToListAsync();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(Guid id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null && reserva.Estado == EstadoReserva.Confirmada)
            {
                reserva.Estado = EstadoReserva.Finalizada;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Reserva marcada como finalizada.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}