using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StayGo.Data;
using StayGo.Models;

// 🚨 PROTEGE EL ÁREA COMPLETA: Solo usuarios con el rol "Admin" pueden acceder a esta área.
[Authorize(Roles = "Admin")]
[Area("Admin")]
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        // ... (Tu lógica de métricas)
        ViewBag.MetricAlojamientos = 128;
        ViewBag.MetricReservas = 23;
        ViewBag.MetricOcupacion = 74;
        ViewBag.MetricIngresos = 12450;
        return View();
    }
}

// Controladores de la sub-área Admin (Asumen [Authorize] del área)

public class PropiedadController : Controller
{
    // ... (Inyección de DbContext o servicios aquí si fuera necesario)
    public IActionResult Index() => View();
    public IActionResult Crear() => View();
    // ... otros métodos
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

// El AccountController debe estar en el área Identity si usa Identity UI por defecto
// Si es un controlador personalizado en la carpeta Admin, es mejor renombrarlo
// Dejo el original simplificado:
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
