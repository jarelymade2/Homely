using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace StayGo.Controllers
{
    public class ResenaController : Controller
    {
        private readonly StayGoContext _db;
        private readonly ILogger<ResenaController> _logger;

        public ResenaController(StayGoContext db, ILogger<ResenaController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // POST: /Resena/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(string propiedadId, int puntuacion, string comentario)
        {
            try
            {
                _logger.LogInformation("=== INICIANDO CREACIÓN DE RESEÑA ===");
                _logger.LogInformation($"PropiedadId recibido: {propiedadId}");
                _logger.LogInformation($"Puntuación: {puntuacion}");
                _logger.LogInformation($"Comentario: {comentario}");

                // Validaciones básicas
                if (string.IsNullOrEmpty(propiedadId))
                {
                    _logger.LogError("ERROR: PropiedadId está vacío");
                    TempData["Error"] = "ID de propiedad no válido.";
                    return RedirectToAction("Index", "Propiedad");
                }

                if (puntuacion < 1 || puntuacion > 5)
                {
                    TempData["Error"] = "La puntuación debe ser entre 1 y 5 estrellas.";
                    return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
                }

                if (string.IsNullOrWhiteSpace(comentario) || comentario.Trim().Length < 5)
                {
                    TempData["Error"] = "El comentario debe tener al menos 5 caracteres.";
                    return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
                }

                // Obtener usuario actual
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Debes iniciar sesión para dejar una reseña.";
                    return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
                }

                _logger.LogInformation($"Usuario ID: {userId}");

                // Buscar la propiedad
                Propiedad? propiedad = null;
                
                // Primero buscar como string
                propiedad = await _db.Propiedades
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id.ToString() == propiedadId);

                // Si no se encuentra, intentar como Guid
                if (propiedad == null && Guid.TryParse(propiedadId, out Guid propiedadIdGuid))
                {
                    _logger.LogInformation($"Buscando propiedad como Guid: {propiedadIdGuid}");
                    propiedad = await _db.Propiedades
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == propiedadIdGuid);
                }

                if (propiedad == null)
                {
                    _logger.LogError($"ERROR: No se encontró propiedad con ID '{propiedadId}'");
                    TempData["Error"] = "La propiedad no existe.";
                    return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
                }

                _logger.LogInformation($"Propiedad encontrada: {propiedad.Titulo} (ID: {propiedad.Id})");

                // Determinar el PropiedadId final (usar el Guid de la propiedad encontrada)
                Guid propiedadIdFinal = propiedad.Id;

                // Verificar si el usuario ya tiene una reseña para esta propiedad
                var reseñaExistente = await _db.Resenas
                    .FirstOrDefaultAsync(r => r.PropiedadId == propiedadIdFinal && r.UsuarioId == userId);

                if (reseñaExistente != null)
                {
                    // Actualizar reseña existente
                    reseñaExistente.Puntuacion = puntuacion;
                    reseñaExistente.Comentario = comentario.Trim();
                    reseñaExistente.Fecha = DateTime.UtcNow;

                    _db.Resenas.Update(reseñaExistente);
                    await _db.SaveChangesAsync();

                    TempData["Success"] = "Tu reseña ha sido actualizada.";
                    _logger.LogInformation("Reseña actualizada exitosamente");
                }
                else
                {
                    // Crear nueva reseña
                    var nuevaResena = new Resena
                    {
                        Id = Guid.NewGuid(),
                        PropiedadId = propiedadIdFinal,
                        UsuarioId = userId,
                        Puntuacion = puntuacion,
                        Comentario = comentario.Trim(),
                        Fecha = DateTime.UtcNow
                    };

                    _db.Resenas.Add(nuevaResena);
                    await _db.SaveChangesAsync();

                    TempData["Success"] = "¡Gracias por dejar tu reseña!";
                    _logger.LogInformation("Nueva reseña creada exitosamente");
                }

                _logger.LogInformation("=== RESEÑA CREADA EXITOSAMENTE ===");
                
                // Redirigir usando el ID original (string) para mantener consistencia
                return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reseña para propiedad {PropiedadId}", propiedadId);
                TempData["Error"] = "Ocurrió un error al guardar tu reseña. Por favor, intenta nuevamente.";
                return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
            }
        }

        // POST: /Resena/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid reseñaIdGuid))
                {
                    TempData["Error"] = "ID de reseña no válido.";
                    return RedirectToAction("Index", "Propiedad");
                }

                var reseña = await _db.Resenas
                    .FirstOrDefaultAsync(r => r.Id == reseñaIdGuid);

                if (reseña == null)
                {
                    TempData["Error"] = "La reseña no existe.";
                    return RedirectToAction("Index", "Propiedad");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var esAdmin = User.IsInRole("Admin");

                if (reseña.UsuarioId != userId && !esAdmin)
                {
                    TempData["Error"] = "No tienes permisos para eliminar esta reseña.";
                    return RedirectToAction("Details", "Propiedad", new { id = reseña.PropiedadId.ToString() });
                }

                var propiedadId = reseña.PropiedadId.ToString();
                _db.Resenas.Remove(reseña);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Reseña eliminada correctamente.";
                return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar reseña {ResenaId}", id);
                TempData["Error"] = "Ocurrió un error al eliminar la reseña.";
                return RedirectToAction("Index", "Propiedad");
            }
        }

        // GET: /Resena/Test
        [AllowAnonymous]
        public IActionResult Test()
        {
            return Content("✅ ResenaController está funcionando correctamente!");
        }
    }
}