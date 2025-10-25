using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayGo.ViewModels;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http; // Necesario para la Session
using System.Text.Json; // Necesario para serializar/deserializar JSON
using StayGo.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace StayGo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StayGoContext _context;
        private const string _historialKey = "HistorialUbicacion";

        public HomeController(ILogger<HomeController> logger, StayGoContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        // --- MÉTODOS PRIVADOS PARA GESTIONAR LA SESIÓN ---
        
        // Carga la lista de ubicaciones guardadas en la sesión
        private List<string> ObtenerHistorial()
        {
            var historialJson = HttpContext.Session.GetString(_historialKey);
            if (string.IsNullOrEmpty(historialJson))
            {
                return new List<string>();
            }
            // Deserializar JSON a List<string>
            // Usamos un operador de coalescencia de nulos para seguridad
            return JsonSerializer.Deserialize<List<string>>(historialJson) ?? new List<string>();
        }

        // Guarda y actualiza la lista en la sesión
        private void AgregarAlHistorial(string ubicacion)
        {
            // La comprobación de nulidad evita la advertencia CS8604
            if (string.IsNullOrWhiteSpace(ubicacion)) return;

            var historial = ObtenerHistorial();
            string ubicacionNormalizada = ubicacion.Trim();

            // 1. Eliminar si ya existe (para moverla al inicio)
            historial.RemoveAll(item => item.Equals(ubicacionNormalizada, StringComparison.OrdinalIgnoreCase));
            
            // 2. Insertar la nueva ubicación al inicio
            historial.Insert(0, ubicacionNormalizada);

            // 3. Limitar a 5 elementos
            if (historial.Count > 5)
            {
                historial.RemoveRange(5, historial.Count - 5);
            }

            // 4. Guardar la lista actualizada en la sesión
            HttpContext.Session.SetString(_historialKey, JsonSerializer.Serialize(historial));
        }

        // --- ACCIONES DEL CONTROLADOR ---

        public IActionResult Index(
            string? q,
            DateTime? checkin,
            DateTime? checkout,
            int adults = 1,
            int children = 0)
        {
            // Pasa los filtros para la persistencia del formulario
            ViewBag.Checkin = checkin;
            ViewBag.Checkout = checkout;
            ViewBag.Adults = adults;
            ViewBag.Children = children;

            // 🛑 Pasa el historial de búsqueda a la vista para el datalist
            ViewBag.HistorialUbicacion = ObtenerHistorial();

            // 🌟 Obtener la propiedad con mejor reseña promedio
            var propiedadDestacada = _context.Propiedades
                .Include(p => p.Resenas)
                .Include(p => p.Imagenes)
                .Where(p => p.Resenas.Any()) // Solo propiedades con reseñas
                .Select(p => new
                {
                    Propiedad = p,
                    PromedioResenas = p.Resenas.Average(r => r.Puntuacion)
                })
                .OrderByDescending(x => x.PromedioResenas)
                .ThenByDescending(x => x.Propiedad.Resenas.Count) // Desempate por cantidad de reseñas
                .FirstOrDefault();

            ViewBag.PropiedadDestacada = propiedadDestacada?.Propiedad;
            ViewBag.PromedioDestacada = propiedadDestacada?.PromedioResenas;

            return View();
        }

        // GET: Home/ResultadoBusqueda
        // Procesa la búsqueda y SIEMPRE regresa al Home/Index con un mensaje.
        public IActionResult ResultadoBusqueda(
            string? q, 
            DateTime? checkin, 
            DateTime? checkout, 
            int adults = 1, 
            int children = 0)
        {
            // 🛑 LÓGICA DE BÚSQUEDA (Reemplaza esta simulación con tu código de DB) 🛑
            int totalEstadiasEncontradas = 0; 
            
            // Si la ubicación es "piura" (ejemplo de éxito)
            if (!string.IsNullOrEmpty(q) && q.Equals("piura", StringComparison.OrdinalIgnoreCase))
            {
                 totalEstadiasEncontradas = 5; 
            }
            // 🛑 FIN DE LÓGICA DE BÚSQUEDA 🛑

            if (totalEstadiasEncontradas > 0)
            {
                // Éxito: Guardar la ubicación en el historial
                AgregarAlHistorial(q ?? string.Empty);
                
                TempData["MensajeBusqueda"] = $"¡Éxito! Encontramos {totalEstadiasEncontradas} estadias que coinciden con tu búsqueda.";
                TempData["MensajeTipo"] = "alert-success";
            }
            else
            {
                // Fracaso: No se guarda nada.
                TempData["MensajeBusqueda"] = "Lo sentimos, no se encontraron estadias que coincidan con tu búsqueda. Intenta con otros filtros.";
                TempData["MensajeTipo"] = "alert-warning";
            }

            // 🛑 Regresamos SIEMPRE al Home/Index, manteniendo los filtros en el URL.
            return RedirectToAction("Index", new { q, checkin, checkout, adults, children });
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
    }
}