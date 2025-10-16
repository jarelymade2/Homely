using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity; // Necesario para UserManager
using StayGo.Models;
using StayGo.ViewModels;
using System.Text.Json; // Necesario para serializar/deserializar la lista
using System.Security.Claims;
using System.Threading.Tasks; // Necesario para usar métodos asíncronos

namespace StayGo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<ApplicationUser> _userManager; // Inyección de UserManager

    public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    // Método privado para obtener el historial de búsqueda del usuario actual
    private async Task<List<string>> GetUserSearchHistoryAsync()
    {
        // Solo obtener historial si el usuario está logueado
        if (User.Identity!.IsAuthenticated)
        {
            // Obtener el objeto de usuario completo
            var user = await _userManager.GetUserAsync(User);
            if (user != null && !string.IsNullOrEmpty(user.SearchHistoryJson))
            {
                // Deserializar el historial guardado en la DB
                return JsonSerializer.Deserialize<List<string>>(user.SearchHistoryJson) ?? new List<string>();
            }
        }
        // Si no está autenticado o no hay historial, devuelve una lista vacía.
        return new List<string>(); 
    }

    [HttpGet]
    public async Task<IActionResult> Index() // Actualizado a async
    {
        // 1. RECUPERAR el historial desde la BASE DE DATOS del usuario autenticado
        var historial = await GetUserSearchHistoryAsync();

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

    // Método que procesa la búsqueda y ACTUALIZA el historial en la BASE DE DATOS
    [HttpGet] 
    public async Task<IActionResult> SearchResults(string location, DateTime checkin, DateTime checkout, int children, int adults) // Actualizado a async
    {
        // 2. LÓGICA DE GESTIÓN DE HISTORIAL EN DB: Solo si el usuario está autenticado
        if (!string.IsNullOrEmpty(location) && User.Identity!.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                // A. Recuperar la lista desde el campo SearchHistoryJson
                var historial = JsonSerializer.Deserialize<List<string>>(user.SearchHistoryJson) ?? new List<string>();
                
                // B. Limpiar y añadir la nueva ubicación al inicio
                historial.Remove(location);
                historial.Insert(0, location); 

                // C. Limitar la lista a un máximo de 5 elementos
                if (historial.Count > 5)
                {
                    historial.RemoveRange(5, historial.Count - 5);
                }

                // D. Serializar y GUARDAR en el modelo de usuario y en la DB
                user.SearchHistoryJson = JsonSerializer.Serialize(historial);
                await _userManager.UpdateAsync(user); 
            }
        }
        
        // Aquí debe ir la lógica real de búsqueda a tu base de datos
        // ... 
        
        return View(); 
    }

    // Redireccionan al área Identity
    public IActionResult Login()
    {
        return RedirectToPage("/Account/Login", new { area = "Identity" });
    }
    
    public IActionResult Register()
    {
        return RedirectToPage("/Account/Register", new { area = "Identity" });
    }
}