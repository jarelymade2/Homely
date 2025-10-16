using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using StayGo.Models;
using StayGo.ViewModels;
using System.Text.Json; 

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
        // 1. RECUPERAR el historial completo de la sesión.
        var historyJson = HttpContext.Session.GetString("BusquedaHistorial");
        
        // Deserializa el JSON a una lista de strings. Si es nulo/vacío, crea una lista nueva.
        var historial = string.IsNullOrEmpty(historyJson) 
                        ? new List<string>() 
                        : JsonSerializer.Deserialize<List<string>>(historyJson) ?? new List<string>();

        // Pasa el historial (últimas 5 búsquedas) a la vista para el datalist.
        ViewBag.HistorialUbicacion = historial;
        
        // Pasa la última búsqueda (el primer elemento) para pre-llenar el campo.
        ViewBag.UltimaUbicacion = historial.FirstOrDefault(); 
        
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

    // Método que procesa la búsqueda y GESTIONA EL HISTORIAL
    [HttpGet] 
    public IActionResult SearchResults(string location, DateTime checkin, DateTime checkout, int children, int adults)
    {
        if (!string.IsNullOrEmpty(location))
        {
            // A. Recuperar la lista existente
            var historyJson = HttpContext.Session.GetString("BusquedaHistorial");
            var historial = string.IsNullOrEmpty(historyJson) 
                            ? new List<string>() 
                            : JsonSerializer.Deserialize<List<string>>(historyJson) ?? new List<string>();

            // B. Asegurar que la ubicación no esté ya en la lista (evitar duplicados)
            historial.Remove(location);

            // C. Añadir la nueva ubicación al inicio (hace que sea la más reciente)
            historial.Insert(0, location); 

            // D. Limitar la lista a un máximo de 5 elementos
            if (historial.Count > 5)
            {
                historial.RemoveRange(5, historial.Count - 5);
            }

            // E. Serializar y guardar la lista actualizada de vuelta en la sesión
            var updatedJson = JsonSerializer.Serialize(historial);
            HttpContext.Session.SetString("BusquedaHistorial", updatedJson);
        }
        
        return View(); 
    }

    public IActionResult Login()
    {
        return RedirectToPage("/Account/Login", new { area = "Identity" });
    }
    
    public IActionResult Register()
    {
        return RedirectToPage("/Account/Register", new { area = "Identity" });
    }
}