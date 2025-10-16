using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // NECESARIO para usar HttpContext.Session
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
        // 1. RECUPERAR DATOS DE SESIÓN para pre-llenar la búsqueda
        // Recupera la ubicación guardada si existe.
        ViewBag.UltimaUbicacion = HttpContext.Session.GetString("UltimaBusquedaUbicacion");
        
        // Aquí podrías recuperar otros filtros (fechas, adultos, etc.) si los guardaste.
        
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

    // 🚨 NOTA: Si usas Identity Razor Pages, estos métodos deben REDIRIGIR al área Identity
    // o deben eliminarse si usas el componente <partial name="_LoginPartial" />

    public IActionResult Login()
    {
        // Redirige a la página de login de Identity
        return RedirectToPage("/Account/Login", new { area = "Identity" });
    }
    
    public IActionResult Register()
    {
        // Redirige a la página de registro de Identity
        return RedirectToPage("/Account/Register", new { area = "Identity" });
    }


    // 2. Método que procesa la búsqueda y GUARDA los datos en la Sesión
    [HttpPost] // Es más común que los formularios de búsqueda sean POST
    public IActionResult SearchResults(string location, DateTime checkin, DateTime checkout, int children, int adults)
    {
        // Guardamos la Ubicación en la Sesión
        if (!string.IsNullOrEmpty(location))
        {
            // Usamos HttpContext.Session.SetString para guardar datos de texto
            HttpContext.Session.SetString("UltimaBusquedaUbicacion", location);
            
            // Opcional: podrías guardar otros datos complejos si fuera necesario
            // HttpContext.Session.SetInt32("UltimosAdultos", adults);
        }
        
        // ... Lógica para buscar alojamientos en la base de datos ...
        
        return View(); // Retorna la vista de resultados
    }
}