using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StayGo.Data;
using StayGo.Models;
[Area("Admin")]
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        // ejemplo de métricas:
        ViewBag.MetricAlojamientos = 128;
        ViewBag.MetricReservas = 23;
        ViewBag.MetricOcupacion = 74;
        ViewBag.MetricIngresos = 12450;
        return View();
    }
}

// AlojamientoController.cs (Admin)
public class PropiedadController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Crear() => View();
}

// ReservaController.cs (Admin)
public class ReservaController : Controller
{
    public IActionResult Index() => View();
}

// UsuarioController.cs
public class UsuarioController : Controller
{
    public IActionResult Index() => View();
}

// ReportesController.cs
public class ReportesController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Export() => File(Array.Empty<byte>(), "text/plain", "report.txt");
}

// AjustesController.cs
public class AjustesController : Controller
{
    public IActionResult Index() => View();
}

// AccountController.cs (para cerrar sesión)
public class AccountController : Controller
{
    [HttpPost]
    public IActionResult Logout()
    {
        // TODO: SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
