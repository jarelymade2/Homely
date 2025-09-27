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
        _db.Habitaciones.Add(hab);
        await _db.SaveChangesAsync();

        return RedirectToAction("Detalles", "Propiedad", new { area = "Admin", id = propiedadId });
    }

    // (Opcional) Editar/Eliminar...
}
