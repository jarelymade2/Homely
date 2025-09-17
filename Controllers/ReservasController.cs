using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models; // Asegúrate de que esta línea esté aquí
using System.Security.Claims;
using System.Threading.Tasks;

namespace StayGo.Controllers;

public class ReservasController : Controller
{
    private readonly StayGoContext _context;
    private readonly UserManager<ApplicationUser> _userManager; // Inyecta el UserManager

    // Agrega el UserManager al constructor
    public ReservasController(StayGoContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Obtener el usuario actualmente autenticado
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            // Si el usuario no está autenticado, redirigir al login
            return RedirectToAction("Login", "Auth");
        }

        // Obtener las reservas del usuario actual
        var reservas = await _context.Reservas
            .Include(r => r.Propiedad)
            .Include(r => r.Habitacion)
            .Where(r => r.UsuarioId == user.Id)
            .ToListAsync();

        return View(reservas);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("PropiedadId,FechaEntrada,FechaSalida")] Reserva reserva)
    {
        if (ModelState.IsValid)
        {
            // Obtener el usuario actualmente autenticado
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            // Asignar el ID del usuario al modelo de reserva
            reserva.UsuarioId = user.Id;
            
            // Aquí iría la lógica adicional de negocio, como la verificación de disponibilidad
            // y el cálculo del precio.
            
            _context.Add(reserva);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        return View(reserva);
    }
}