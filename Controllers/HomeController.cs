using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;          // Session
using System.Text.Json;                   // JSON para historial
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;             // para obtener el usuario
using StayGo.Data;                        // DbContext
using StayGo.Models;                      // Propiedad, etc.
using StayGo.Services.ML;                 // tu servicio de ML

namespace StayGo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StayGoContext _context;
        private readonly MLRecommendationService _mlService;
        private const string _historialKey = "HistorialUbicacion";

        public HomeController(
            ILogger<HomeController> logger,
            StayGoContext context,
            MLRecommendationService mlService)
        {
            _logger = logger;
            _context = context;
            _mlService = mlService;
        }

        // =========================
        //  MÉTODOS DE SESIÓN
        // =========================
        private List<string> ObtenerHistorial()
        {
            var historialJson = HttpContext.Session.GetString(_historialKey);
            if (string.IsNullOrEmpty(historialJson))

                return new List<string>();

            return JsonSerializer.Deserialize<List<string>>(historialJson) ?? new List<string>();
        }


        private void AgregarAlHistorial(string ubicacion)
        {

            if (string.IsNullOrWhiteSpace(ubicacion)) return;

            var historial = ObtenerHistorial();
            string ubicacionNormalizada = ubicacion.Trim();

            // eliminar duplicados (case-insensitive)
            historial.RemoveAll(item => item.Equals(ubicacionNormalizada, StringComparison.OrdinalIgnoreCase));

            // insertar al inicio
            historial.Insert(0, ubicacionNormalizada);

            // limitar a 5
            if (historial.Count > 5)

                historial.RemoveRange(5, historial.Count - 5);
                

            HttpContext.Session.SetString(_historialKey, JsonSerializer.Serialize(historial));
        }

        // =========================
        //  ACCIONES
        // =========================

        // GET: /
        public async Task<IActionResult> Index(
            string? q,
            DateTime? checkin,
            DateTime? checkout,
            int adults = 1,
            int children = 0)
        {
            // 1. Cargar TODAS las propiedades necesarias (pero NO las mandamos como modelo)
            var todas = await _context.Propiedades
                .Include(p => p.Direccion)
                .Include(p => p.Imagenes)
                .Include(p => p.Resenas)
                .ToListAsync();

            // 2. Destacadas = mejor rating
            var destacadas = todas
                .Where(p => p.Resenas != null && p.Resenas.Any())
                .Select(p => new
                {
                    Prop = p,
                    Promedio = p.Resenas.Average(r => r.Puntuacion)
                })
                .OrderByDescending(x => x.Promedio)
                .ThenByDescending(x => x.Prop.Resenas.Count)
                .Take(4)
                .Select(x => x.Prop)
                .ToList();

            // 3. Recomendaciones solo si está logueado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recomendaciones = new List<Propiedad>();

            if (!string.IsNullOrEmpty(userId))
            {
                // usa tu servicio de ML
                recomendaciones = _mlService.RecommendForUser(userId, topN: 6);
            }

            // 4. Pasar filtros para que el formulario del hero los muestre
            ViewBag.Checkin = checkin;
            ViewBag.Checkout = checkout;
            ViewBag.Adults = adults;
            ViewBag.Children = children;

            // 5. Historial para el datalist
            ViewBag.HistorialUbicacion = ObtenerHistorial();

            // 6. Pasar las listas que la vista va a pintar
            ViewBag.Destacadas = destacadas;
            ViewBag.Recomendaciones = recomendaciones;

            // ❗ Importante: NO mandamos lista como modelo, porque no quieres “todas las propiedades”
            return View();
        }

        // GET: /Home/ResultadoBusqueda
        // Esta acción SOLO valida, guarda historial y pone TempData. Luego vuelve al Index.
        [HttpGet]
        public async Task<IActionResult> ResultadoBusqueda(
            string? q,
            DateTime? checkin,
            DateTime? checkout,
            int adults = 1,
            int children = 0)
        {
            var query = _context.Propiedades
                .Include(p => p.Direccion)
                .Include(p => p.Reservas)
                .AsQueryable();

            // filtro por texto
            if (!string.IsNullOrWhiteSpace(q))
            {
                var qLower = q.Trim().ToLower();
                query = query.Where(p =>
                    p.Titulo.ToLower().Contains(qLower) ||
                    p.Descripcion.ToLower().Contains(qLower) ||
                    (p.Direccion != null &&
                        (
                            p.Direccion.Pais.ToLower().Contains(qLower) ||
                            p.Direccion.Ciudad.ToLower().Contains(qLower) ||
                            p.Direccion.Linea1.ToLower().Contains(qLower) ||
                            (p.Direccion.Linea2 != null && p.Direccion.Linea2.ToLower().Contains(qLower))
                        )
                    )
                );
            }

            // filtro por capacidad
            int totalPersonas = adults + children;
            query = query.Where(p => !p.Capacidad.HasValue || p.Capacidad.Value >= totalPersonas);

            // filtro por fechas
            if (checkin.HasValue && checkout.HasValue && checkin < checkout)
            {
                var ci = DateOnly.FromDateTime(checkin.Value.Date);
                var co = DateOnly.FromDateTime(checkout.Value.Date);

                query = query.Where(p =>
                    !p.Reservas.Any(r => r.CheckIn < co && r.CheckOut > ci)
                );
            }

            var resultados = await query.CountAsync();

            if (resultados > 0)
            {
                if (!string.IsNullOrWhiteSpace(q))
                    AgregarAlHistorial(q);

                TempData["MensajeBusqueda"] = $"¡Éxito! Encontramos {resultados} estadías que coinciden con tu búsqueda.";
                TempData["MensajeTipo"] = "alert-success";
            }
            else
            {
                TempData["MensajeBusqueda"] = "Lo sentimos, no se encontraron estadías que coincidan con tu búsqueda. Intenta con otros filtros.";
                TempData["MensajeTipo"] = "alert-warning";
            }

            // volvemos al Index para que tu diseño se vea igual
            return RedirectToAction(nameof(Index), new { q, checkin, checkout, adults, children });
        }
    }
}
