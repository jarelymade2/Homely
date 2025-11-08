using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.ValueObjects;

namespace StayGo.Areas.Admin.Controllers;

[Area("Admin")]

[Route("Admin/[controller]/[action]")]

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
            .ThenInclude(h => h.Imagenes)
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id);

    if (prop == null) return NotFound();
    return View(prop);
}


    // GET: /Admin/Propiedad/Editar/{id}
    [HttpGet]
    public async Task<IActionResult> Editar(Guid id)
    {
        var prop = await _db.Propiedades
            .Include(p => p.Direccion)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prop == null) return NotFound();
        return View(prop);
    }

    // POST: /Admin/Propiedad/Editar/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, Propiedad prop)
    {
        if (id != prop.Id) return BadRequest();
        prop.Direccion ??= new Direccion();

        if (!ModelState.IsValid) return View(prop);

        var entity = await _db.Propiedades
            .Include(p => p.Direccion)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null) return NotFound();

        // Campos básicos
        entity.Titulo = prop.Titulo;
        entity.Descripcion = prop.Descripcion;
        entity.Tipo = prop.Tipo;
        entity.PrecioPorNoche = prop.PrecioPorNoche;
        entity.Capacidad = prop.Capacidad;
        entity.Lat = prop.Lat;
        entity.Lng = prop.Lng;

        // Dirección
        entity.Direccion ??= new Direccion();
        entity.Direccion.Ciudad = prop.Direccion.Ciudad;
        entity.Direccion.Pais = prop.Direccion.Pais;
        entity.Direccion.Linea1 = prop.Direccion.Linea1;
        entity.Direccion.Linea2 = prop.Direccion.Linea2;
        entity.Direccion.CodigoPostal = prop.Direccion.CodigoPostal;

        await _db.SaveChangesAsync();
        TempData["Ok"] = "Propiedad actualizada.";
        return RedirectToAction(nameof(Detalles), new { id });
    }

    // GET: /Admin/Propiedad/Eliminar/{id}
    [HttpGet]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var prop = await _db.Propiedades
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
        if (prop == null) return NotFound();
        return View(prop);
    }

    // POST: /Admin/Propiedad/Eliminar/{id}
    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(Guid id)
    {
        var prop = await _db.Propiedades.FindAsync(id);
        if (prop != null)
        {
            _db.Propiedades.Remove(prop);   // cascada elimina imágenes/habitaciones si lo configuraste
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Propiedad eliminada.";
        }
        return RedirectToAction(nameof(Index));
    }
}
