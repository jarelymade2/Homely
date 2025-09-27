using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;

namespace StayGo.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/Propiedad/{propiedadId:guid}/[controller]/[action]")]
public class ImagenPropiedadController : Controller
{
    private readonly StayGoContext _db;
    public ImagenPropiedadController(StayGoContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(Guid propiedadId)
    {
        var prop = await _db.Propiedades
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.Id == propiedadId);
        if (prop == null) return NotFound();
        return View(prop); // la vista muestra la galería y botones
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(Guid propiedadId, string url, bool esPrincipal)
    {
        var prop = await _db.Propiedades.Include(p => p.Imagenes).FirstOrDefaultAsync(p => p.Id == propiedadId);
        if (prop == null) return NotFound();

        if (string.IsNullOrWhiteSpace(url))
        {
            TempData["Err"] = "URL requerida.";
            return RedirectToAction(nameof(Index), new { propiedadId });
        }

        if (esPrincipal)
            foreach (var img in prop.Imagenes) img.EsPrincipal = false;

        prop.Imagenes.Add(new ImagenPropiedad
        {
            Id = Guid.NewGuid(),
            PropiedadId = propiedadId,
            Url = url.Trim(),
            EsPrincipal = esPrincipal
        });

        await _db.SaveChangesAsync();
        TempData["Ok"] = "Imagen agregada.";
        return RedirectToAction(nameof(Index), new { propiedadId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(Guid propiedadId, Guid id)
    {
        var img = await _db.ImagenesPropiedad.FirstOrDefaultAsync(i => i.Id == id && i.PropiedadId == propiedadId);
        if (img != null)
        {
            _db.Remove(img);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Imagen eliminada.";
        }
        return RedirectToAction(nameof(Index), new { propiedadId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarPrincipal(Guid propiedadId, Guid id)
    {
        var prop = await _db.Propiedades.Include(p => p.Imagenes).FirstOrDefaultAsync(p => p.Id == propiedadId);
        if (prop == null) return NotFound();

        foreach (var img in prop.Imagenes) img.EsPrincipal = img.Id == id;
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Imagen principal actualizada.";
        return RedirectToAction(nameof(Index), new { propiedadId });
    }
}
