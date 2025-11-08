using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;

namespace StayGo.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/Propiedad/{propiedadId:guid}/[controller]/[action]")]
public class HabitacionController : Controller
{
    private readonly StayGoContext _db;
    public HabitacionController(StayGoContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Crear(Guid propiedadId)
    {
        var prop = await _db.Propiedades.AsNoTracking().FirstOrDefaultAsync(p => p.Id == propiedadId);
        if (prop == null) return NotFound();
        ViewBag.Propiedad = prop;
        return View(new Habitacion { PropiedadId = propiedadId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Guid propiedadId, Habitacion hab)
    {
        if (propiedadId != hab.PropiedadId) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Propiedad = await _db.Propiedades.AsNoTracking().FirstOrDefaultAsync(p => p.Id == propiedadId);
            return View(hab);
        }

        hab.Id = Guid.NewGuid();

        // Procesar imágenes
        var imagenes = (hab.Imagenes ?? new List<ImagenHabitacion>())
            .Where(i => i != null && !string.IsNullOrWhiteSpace(i.Url))
            .Select(i => { i.Url = i.Url.Trim(); i.Id = Guid.NewGuid(); i.HabitacionId = hab.Id; return i; })
            .ToList();

        // Si ninguna está marcada como principal, marca la primera
        if (imagenes.Count > 0 && !imagenes.Any(i => i.EsPrincipal))
            imagenes[0].EsPrincipal = true;

        hab.Imagenes = imagenes;

        _db.Habitaciones.Add(hab);
        await _db.SaveChangesAsync();

        return RedirectToAction("Detalles", "Propiedad", new { area = "Admin", id = propiedadId });
    }

    // GET: Editar
    [HttpGet]
    public async Task<IActionResult> Editar(Guid propiedadId, Guid id)
    {
        var hab = await _db.Habitaciones
            .Include(h => h.Imagenes)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id && h.PropiedadId == propiedadId);
        if (hab == null) return NotFound();

        ViewBag.Propiedad = await _db.Propiedades.AsNoTracking().FirstOrDefaultAsync(p => p.Id == propiedadId);
        return View(hab);
    }

    // POST: Editar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid propiedadId, Guid id, Habitacion hab)
    {
        if (id != hab.Id || propiedadId != hab.PropiedadId) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Propiedad = await _db.Propiedades.AsNoTracking().FirstOrDefaultAsync(p => p.Id == propiedadId);
            return View(hab);
        }

        var entity = await _db.Habitaciones.FirstOrDefaultAsync(h => h.Id == id);
        if (entity == null) return NotFound();

        // Actualizar campos básicos
        entity.Nombre = hab.Nombre;
        entity.Capacidad = hab.Capacidad;
        entity.PrecioPorNoche = hab.PrecioPorNoche;
        entity.Piso = hab.Piso;
        entity.Numero = hab.Numero;
        entity.TipoCama = hab.TipoCama;

        // Procesar imágenes
        var nuevasImagenes = (hab.Imagenes ?? new List<ImagenHabitacion>())
            .Where(i => i != null && !string.IsNullOrWhiteSpace(i.Url))
            .Select(i => new ImagenHabitacion
            {
                Id = Guid.NewGuid(),
                Url = i.Url.Trim(),
                HabitacionId = id,
                EsPrincipal = i.EsPrincipal
            })
            .ToList();

        // Si ninguna está marcada como principal, marca la primera
        if (nuevasImagenes.Count > 0 && !nuevasImagenes.Any(i => i.EsPrincipal))
            nuevasImagenes[0].EsPrincipal = true;

        // Eliminar imágenes antiguas de la BD
        await _db.ImagenesHabitacion.Where(i => i.HabitacionId == id).ExecuteDeleteAsync();

        // Agregar nuevas imágenes
        await _db.ImagenesHabitacion.AddRangeAsync(nuevasImagenes);
        await _db.SaveChangesAsync();

        TempData["ok"] = "Habitación actualizada.";
        return RedirectToAction("Detalles", "Propiedad", new { area = "Admin", id = propiedadId });
    }

    // GET: Eliminar (confirmación)
    [HttpGet]
    public async Task<IActionResult> Eliminar(Guid propiedadId, Guid id)
    {
        var hab = await _db.Habitaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id && h.PropiedadId == propiedadId);
        if (hab == null) return NotFound();

        ViewBag.Propiedad = await _db.Propiedades.AsNoTracking().FirstOrDefaultAsync(p => p.Id == propiedadId);
        return View(hab);
    }

    // POST: Eliminar
    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(Guid propiedadId, Guid id)
    {
        var hab = await _db.Habitaciones.FirstOrDefaultAsync(h => h.Id == id && h.PropiedadId == propiedadId);
        if (hab == null) return NotFound();

        // Restricciones: si hay reservas/disponibilidades, no se puede borrar (tienes DeleteBehavior.Restrict)
        var tieneReservas = await _db.Reservas.AnyAsync(r => r.HabitacionId == id);
        var tieneDisp = await _db.Disponibilidades.AnyAsync(d => d.HabitacionId == id);
        if (tieneReservas || tieneDisp)
        {
            TempData["err"] = "No puedes eliminar esta habitación porque tiene reservas o disponibilidades asociadas.";
            return RedirectToAction("Detalles", "Propiedad", new { area = "Admin", id = propiedadId });
        }

        _db.Habitaciones.Remove(hab);
        await _db.SaveChangesAsync();

        TempData["ok"] = "Habitación eliminada.";
        return RedirectToAction("Detalles", "Propiedad", new { area = "Admin", id = propiedadId });
    }
}
