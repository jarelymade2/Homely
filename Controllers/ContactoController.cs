using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StayGo.Data;
using StayGo.Models;

namespace StayGo.Controllers;

public class ContactoController : Controller
{
    private readonly StayGoContext _db;
    private readonly UserManager<ApplicationUser> _um;
    private readonly ILogger<ContactoController> _logger;

    public ContactoController(StayGoContext db, UserManager<ApplicationUser> um, ILogger<ContactoController> logger)
    {
        _db = db;
        _um = um;
        _logger = logger;
    }

    // GET /Contacto
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new ContactoMensaje();

        if (User.Identity?.IsAuthenticated == true)
        {
            var me = await _um.GetUserAsync(User); // ApplicationUser
            if (me != null)
            {
                model.IdentityUserId = me.Id;
                model.Email = me.Email ?? "";
                // Usa Nombre (de ApplicationUser) o UserName como fallback
                model.Nombre = string.IsNullOrWhiteSpace(me.FirstName) ? (me.UserName ?? "") : me.FirstName;
            }
        }

        return View(model);
    }

    // POST /Contacto
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactoMensaje model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var me = await _um.GetUserAsync(User);
            if (me != null)
            {
                model.IdentityUserId = me.Id;

                if (string.IsNullOrWhiteSpace(model.Email))
                    model.Email = me.Email ?? model.Email;

                if (string.IsNullOrWhiteSpace(model.Nombre))
                    model.Nombre = string.IsNullOrWhiteSpace(me.FirstName) ? (me.UserName ?? model.Nombre) : me.FirstName;
            }
        }

        if (!ModelState.IsValid)
            return View(model);

        _db.Contactos.Add(model);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Contacto registrado: {@Contacto}", model);
        TempData["ok"] = "¡Gracias! Tu mensaje fue enviado.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Contacto/Lista  (solo Admin)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Lista()
    {
        var msgs = await _db.Contactos
            .AsNoTracking()
            .OrderByDescending(x => x.FechaUtc)
            .Take(100)
            .ToListAsync();

        return View(msgs);
    }
}
