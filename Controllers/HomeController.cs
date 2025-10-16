using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // Necesario para usar HttpContext.Session
using StayGo.Models;
using StayGo.ViewModels;

namespace StayGo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        // RECUPERAR DATOS DE SESIÓN para pre-llenar la búsqueda
        // Si existe en la sesión, se pasa a la vista; si no, es null.
        ViewBag.UltimaUbicacion = HttpContext.Session.GetString("UltimaBusquedaUbicacion");
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Método para manejar la búsqueda de resultados (solía dar 405)
    [HttpGet] // CORRECCIÓN CLAVE: Permite que el formulario de búsqueda use el método GET por defecto.
    public IActionResult SearchResults(string location, DateTime checkin, DateTime checkout, int children, int adults)
    {
        // 1. GUARDAR la Ubicación en la Sesión (usando HttpContext.Session)
        if (!string.IsNullOrEmpty(location))
        {
            HttpContext.Session.SetString("UltimaBusquedaUbicacion", location);
        }
        
        // Aquí debes incluir la lógica real de búsqueda a tu base de datos
        // ... 
        
        return View(); // Retorna la vista de resultados (SearchResults.cshtml)
    }

    // Redireccionan al área Identity (ya que están fuera de Identity)
    public IActionResult Login()
    {
        return RedirectToPage("/Account/Login", new { area = "Identity" });
    }
    
    public IActionResult Register()
    {
        return RedirectToPage("/Account/Register", new { area = "Identity" });
    }
}