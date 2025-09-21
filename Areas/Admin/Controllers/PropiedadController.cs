using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.ValueObjects;

namespace StayGo.Areas.Admin.Controllers;

[Area("Admin")]
public class PropiedadController : Controller
{
    private readonly StayGoContext _db;
    public PropiedadController(StayGoContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var propiedades = await _db.Propiedades
            .Include(p => p.Direccion)
            .Include(p => p.Imagenes)
            .AsNoTracking()
            .OrderByDescending(p => p.Id)
            .ToListAsync();
        return View(propiedades);
    }

    [HttpGet]
    public IActionResult Crear() => View(new Propiedad());
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Propiedad prop)
    {
        // Asegura objetos anidados
        prop.Direccion ??= new Direccion();

        // Normaliza la colección (puede venir null o con items sin URL)
        var imgs = (prop.Imagenes ?? new List<ImagenPropiedad>())
            .Where(i => i != null && !string.IsNullOrWhiteSpace(i.Url))
            .Select(i => { i.Url = i.Url.Trim(); return i; })
            .ToList();

        // Si exiges al menos una imagen, valida:
        if (imgs.Count == 0)
            ModelState.AddModelError("Imagenes", "Agrega al menos una imagen (URL).");

        // Si ninguna está marcada como principal, marca la primera
        if (imgs.Count > 0 && !imgs.Any(i => i.EsPrincipal))
            imgs[0].EsPrincipal = true;

        // Reasigna la colección normalizada al modelo
        prop.Imagenes = imgs;

        if (!ModelState.IsValid)
            return View(prop);

        prop.Id = Guid.NewGuid();
        _db.Propiedades.Add(prop);
        await _db.SaveChangesAsync();

        TempData["Ok"] = "Propiedad creada correctamente.";
        return RedirectToAction(nameof(Index));
    }
[HttpGet]
public async Task<IActionResult> Detalles(Guid id)
{
    var prop = await _db.Propiedades
        .Include(p => p.Direccion)
        .Include(p => p.Imagenes)
        .Include(p => p.Habitaciones)  
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id);

    if (prop == null) return NotFound();
    return View(prop);
}



}
