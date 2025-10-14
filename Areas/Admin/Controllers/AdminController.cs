using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StayGo.Data;
using StayGo.Models;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        
        ViewBag.MetricAlojamientos = 128;
        ViewBag.MetricReservas = 23;
        ViewBag.MetricOcupacion = 74;
        ViewBag.MetricIngresos = 12450;
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
