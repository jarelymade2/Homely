using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;

[Authorize] // requiere estar autenticado para crear/borrar reseñas
public class ResenaController : Controller
{
    private readonly StayGoContext _db;
    private readonly ILogger<ResenaController> _logger;

    public ResenaController(StayGoContext db, ILogger<ResenaController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // POST: Resena/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid propiedadId, int puntuacion, string comentario)
    {
        // Validaciones servidor
        if (puntuacion < 1 || puntuacion > 5)
        {
            TempData["ResenaError"] = "La puntuación debe ser entre 1 y 5.";
            return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
        }

        if (string.IsNullOrWhiteSpace(comentario))
        {
            TempData["ResenaError"] = "El comentario no puede estar vacío.";
            return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ResenaError"] = "Usuario no identificado.";
            return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
        }

        // Verificar propiedad existe
        var propiedad = await _db.Propiedades.AsNoTracking().FirstOrDefaultAsync(p => p.Id == propiedadId);
        if (propiedad == null) return NotFound();

        // Evitar duplicados: un usuario = una reseña por propiedad (opcional; puedes permitir múltiples y cambiarlas)
        var existente = await _db.Resenas
            .FirstOrDefaultAsync(r => r.PropiedadId == propiedadId && r.UsuarioId == userId);

        if (existente != null)
        {
            // Actualizar reseña existente (en lugar de crear otra)
            existente.Puntuacion = puntuacion;
            existente.Comentario = comentario.Trim();
            existente.Fecha = DateTime.UtcNow;

            _db.Resenas.Update(existente);
            await _db.SaveChangesAsync();

            TempData["ResenaSuccess"] = "Tu reseña fue actualizada.";
            return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
        }

        // Crear nueva reseña
        var resena = new Resena
        {
            Id = Guid.NewGuid(),
            PropiedadId = propiedadId,
            UsuarioId = userId,
            Puntuacion = puntuacion,
            Comentario = comentario.Trim(),
            Fecha = DateTime.UtcNow
        };

        _db.Resenas.Add(resena);
        await _db.SaveChangesAsync();

        TempData["ResenaSuccess"] = "Gracias por dejar tu opinión.";
        return RedirectToAction("Details", "Propiedad", new { id = propiedadId });
    }

    // POST: Resena/Delete
    // Solo autor o admin puede borrar — aquí permito al autor o usuario con role Admin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var resena = await _db.Resenas.FirstOrDefaultAsync(r => r.Id == id);
        if (resena == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userIsAdmin = User.IsInRole("Admin");

        if (resena.UsuarioId != userId && !userIsAdmin)
            return Forbid();

        _db.Resenas.Remove(resena);
        await _db.SaveChangesAsync();

        TempData["ResenaSuccess"] = "Reseña eliminada.";
        return RedirectToAction("Details", "Propiedad", new { id = resena.PropiedadId });
    }
}
