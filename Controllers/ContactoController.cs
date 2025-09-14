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
    private readonly UserManager<IdentityUser> _um;
    private readonly ILogger<ContactoController> _logger;

    public ContactoController(StayGoContext db, UserManager<IdentityUser> um, ILogger<ContactoController> logger)
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
            var identityUser = await _um.GetUserAsync(User);
            if (identityUser != null)
            {
                // Prefill desde Identity / Usuario de dominio si existe
                model.IdentityUserId = identityUser.Id;
                model.Email = identityUser.Email ?? "";

                var usuario = await _db.Usuarios.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

                model.Nombre = usuario?.Nombre ?? identityUser.UserName ?? "";
                model.UsuarioId = usuario?.Id;
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
            // Refuerza el vínculo por seguridad (no confíes en values posteados)
            var identityUser = await _um.GetUserAsync(User);
            if (identityUser != null)
            {
                model.IdentityUserId = identityUser.Id;
                if (string.IsNullOrWhiteSpace(model.Email))
                    model.Email = identityUser.Email ?? model.Email;

                var usuario = await _db.Usuarios
                    .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

                if (usuario != null)
                {
                    model.UsuarioId = usuario.Id;
                    if (string.IsNullOrWhiteSpace(model.Nombre))
                        model.Nombre = usuario.Nombre ?? model.Nombre;
                }
            }
        }

        if (!ModelState.IsValid)
            return View(model);

        _db.Contactos.Add(model);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Contacto registrado: {@ContactoMensaje}", model);

        TempData["ok"] = "¡Gracias! Tu mensaje fue enviado.";
        return RedirectToAction(nameof(Index)); // PRG
    }

    // Listado para demo/admin (solo Admin)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Lista()
    {
        var msgs = await _db.Contactos
            .Include(c => c.Usuario)
            .AsNoTracking()
            .OrderByDescending(x => x.FechaUtc)
            .Take(100)
            .ToListAsync();

        return View(msgs);
    }
}
