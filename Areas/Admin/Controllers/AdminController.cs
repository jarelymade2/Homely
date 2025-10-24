using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StayGo.Data;
using StayGo.Models;
using Microsoft.EntityFrameworkCore; // <-- 1. Importar Entity Framework
using System.Threading.Tasks;
using StayGo.Models.Enums;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class AdminController : Controller
{
    private readonly StayGoContext _context;

    public AdminController(StayGoContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index() // 4. Convertir a async Task
    {
        {
            // 5. Definir la fecha de "hoy"
            // Usamos DateOnly porque tu modelo Reserva parece usarlo
            var today = DateOnly.FromDateTime(DateTime.Today);

            // 6. Consultar la base de datos real
            int totalPropiedades = await _context.Propiedades.CountAsync();

            int reservasHoy = await _context.Reservas
                .Where(r => r.CheckIn == today && r.Estado == EstadoReserva.Confirmada) // Solo reservas confirmadas para hoy
                .CountAsync();

            decimal ingresosHoy = await _context.Reservas
                .Where(r => r.CheckIn == today && r.Estado == EstadoReserva.Confirmada)
                .SumAsync(r => r.PrecioTotal);

            int usuariosActivos = await _context.Usuarios.CountAsync(); // Total de usuarios en tu tabla 'Usuario'

            // 7. Usar los nombres CORRECTOS que la vista espera
            ViewBag.TotalPropiedades = totalPropiedades;
            ViewBag.ReservasHoy = reservasHoy;
            ViewBag.UsuariosActivos = usuariosActivos;
            ViewBag.IngresosHoy = ingresosHoy.ToString("N2"); // Formatear como 12345.00

            return View();
        }
    }



    public class PropiedadController : Controller
    {

        public IActionResult Index() => View();
        public IActionResult Crear() => View();

    }

    public class ReservaController : Controller
    {
        public IActionResult Index() => View();
    }

    public class UsuarioController : Controller
    {
        public IActionResult Index() => View();
    }

    public class ReportesController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Export() => File(Array.Empty<byte>(), "text/plain", "report.txt");
    }

    public class AjustesController : Controller
    {
        public IActionResult Index() => View();
    }


    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" }); // Redirecciona a la Home pública
        }
    }
}
