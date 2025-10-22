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

    // GET: /Admin/ImagenPropiedad/Index?propiedadId=...
    [HttpGet]
    public async Task<IActionResult> Index(Guid propiedadId)
    {
        var prop = await _db.Propiedades
            .Include(p => p.Imagenes)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == propiedadId);

        if (prop == null) return NotFound();
        return View(prop);
    }

    // POST: /Admin/ImagenPropiedad/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Guid propiedadId, string url, bool esPrincipal = false)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            TempData["err"] = "La URL es requerida.";
            return RedirectToAction(nameof(Index), new { propiedadId });
        }

        // Si marcarás principal, desmarca las demás primero
        if (esPrincipal)
        {
            await _db.ImagenesPropiedad
                .Where(i => i.PropiedadId == propiedadId && i.EsPrincipal)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.EsPrincipal, false));
        }

        var img = new ImagenPropiedad
        {
            Id = Guid.NewGuid(),
            PropiedadId = propiedadId,
            Url = url.Trim(),
            EsPrincipal = esPrincipal
        };

        // Si no hay ninguna imagen aún, esta se vuelve principal
        var tienePrincipal = await _db.ImagenesPropiedad
            .AnyAsync(i => i.PropiedadId == propiedadId && i.EsPrincipal);
        if (!tienePrincipal) img.EsPrincipal = true;

        _db.ImagenesPropiedad.Add(img);
        await _db.SaveChangesAsync();

        TempData["ok"] = "Imagen agregada.";
        return RedirectToAction(nameof(Index), new { propiedadId });
    }

    // POST: /Admin/ImagenPropiedad/MarcarPrincipal
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarPrincipal(Guid id)
    {
        var img = await _db.ImagenesPropiedad.FirstOrDefaultAsync(x => x.Id == id);
        if (img == null) return NotFound();

        await _db.ImagenesPropiedad
            .Where(i => i.PropiedadId == img.PropiedadId && i.EsPrincipal)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.EsPrincipal, false));

        img.EsPrincipal = true;
        await _db.SaveChangesAsync();

        TempData["ok"] = "Imagen marcada como principal.";
        return RedirectToAction(nameof(Index), new { propiedadId = img.PropiedadId });
    }

    // POST: /Admin/ImagenPropiedad/Eliminar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var img = await _db.ImagenesPropiedad.FirstOrDefaultAsync(x => x.Id == id);
        if (img == null) return NotFound();

        var propiedadId = img.PropiedadId;
        var eraPrincipal = img.EsPrincipal;

        _db.ImagenesPropiedad.Remove(img);
        await _db.SaveChangesAsync();

        // Si borraste la principal, promueve otra si existe
        if (eraPrincipal)
        {
            var otra = await _db.ImagenesPropiedad
                .Where(i => i.PropiedadId == propiedadId)
                .OrderBy(i => i.Id)
                .FirstOrDefaultAsync();

            if (otra != null)
            {
                otra.EsPrincipal = true;
                await _db.SaveChangesAsync();
            }
        }

        TempData["ok"] = "Imagen eliminada.";
        return RedirectToAction(nameof(Index), new { propiedadId });
    }
    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Actualizar(Guid propiedadId, Guid id, string url, bool esPrincipal = false)
{
    if (string.IsNullOrWhiteSpace(url))
    {
        TempData["err"] = "La URL es requerida.";
        return RedirectToAction(nameof(Index), new { propiedadId });
    }

    var img = await _db.ImagenesPropiedad
        .FirstOrDefaultAsync(i => i.Id == id && i.PropiedadId == propiedadId);
    if (img == null) return NotFound();

    img.Url = url.Trim();

    if (esPrincipal && !img.EsPrincipal)
    {
        // deja esta como única principal
        await _db.ImagenesPropiedad
            .Where(i => i.PropiedadId == propiedadId && i.EsPrincipal)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.EsPrincipal, false));

        img.EsPrincipal = true;
    }
    else if (!esPrincipal && img.EsPrincipal)
    {
        // evita quedarte sin principal
        bool hayOtra = await _db.ImagenesPropiedad
            .AnyAsync(i => i.PropiedadId == propiedadId && i.Id != id);
        if (!hayOtra)
        {
            TempData["err"] = "Debe existir al menos una imagen principal.";
            return RedirectToAction(nameof(Index), new { propiedadId });
        }
        img.EsPrincipal = false;
    }

    await _db.SaveChangesAsync();
    TempData["ok"] = "Imagen actualizada.";
    return RedirectToAction(nameof(Index), new { propiedadId });
}
}
