using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;

namespace StayGo.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ContactoController : Controller
{
    private readonly StayGoContext _db;
    private readonly ILogger<ContactoController> _logger;

    public ContactoController(StayGoContext db, ILogger<ContactoController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: /Admin/Contacto/Lista
    public async Task<IActionResult> Index()
    {
        var mensajes = await _db.Contactos
            .AsNoTracking()
            .OrderByDescending(x => x.FechaUtc)
            .Take(200)
            .ToListAsync();

        ViewBag.TotalMensajes = await _db.Contactos.CountAsync();
        return View(mensajes);
    }

    // GET: /Admin/Contacto/Detalles/{id}
    public async Task<IActionResult> Detalles(Guid id)
    {
        var mensaje = await _db.Contactos
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mensaje == null)
        {
            TempData["error"] = "Mensaje no encontrado";
            return RedirectToAction(nameof(Index));
        }

        return View(mensaje);
    }

    // POST: /Admin/Contacto/Eliminar/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var mensaje = await _db.Contactos.FindAsync(id);
        if (mensaje == null)
        {
            TempData["error"] = "Mensaje no encontrado";
            return RedirectToAction(nameof(Index));
        }

        _db.Contactos.Remove(mensaje);
        await _db.SaveChangesAsync();

        TempData["success"] = "Mensaje eliminado correctamente";
        return RedirectToAction(nameof(Index));
    }
}