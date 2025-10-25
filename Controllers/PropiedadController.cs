using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums;
using StayGo.Integration;
using StayGo.Integration; // ← se añade para usar la API del clima y Unsplash

namespace StayGo.Controllers
{
    public class PropiedadController : Controller
    {
        private readonly StayGoContext _db;
        private readonly OpenWeatherIntegration _openWeather;

        public PropiedadController(StayGoContext db, OpenWeatherIntegration openWeather)
        private readonly OpenWeatherIntegration _openWeather; // ← agregado
        private readonly UnsplashIntegration _unsplash; // ← agregado

        // Constructor con inyección del servicio del clima y Unsplash
        public PropiedadController(
            StayGoContext db,
            OpenWeatherIntegration openWeather,
            UnsplashIntegration unsplash) // ← agregado
        {
            _db = db;
            _openWeather = openWeather;
            _unsplash = unsplash; // ← agregado
        }

        // GET: /Propiedad
        [HttpGet]
        public async Task<IActionResult> Index(
            string? q,
            TipoPropiedad? tipo,
            string? ciudad,
            decimal? min,
            decimal? max,
            string? orden = "recientes",
            int page = 1,
            int pageSize = 9)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 60) pageSize = 9;

            IQueryable<Propiedad> query = _db.Propiedades
                .Include(p => p.Imagenes)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qLike = $"%{q.Trim()}%";
                query = query.Where(p =>
                    EF.Functions.Like(p.Titulo, qLike) ||
                    (p.Descripcion != null && EF.Functions.Like(p.Descripcion, qLike)) ||
                    (p.Direccion != null && (
                        EF.Functions.Like(p.Direccion.Ciudad ?? "", qLike) ||
                        EF.Functions.Like(p.Direccion.Pais ?? "", qLike) ||
                        EF.Functions.Like(p.Direccion.Linea1 ?? "", qLike) ||
                        EF.Functions.Like(p.Direccion.Linea2 ?? "", qLike) ||
                        EF.Functions.Like(p.Direccion.CodigoPostal ?? "", qLike)
                    ))
                );
            }

            if (tipo.HasValue)
                query = query.Where(p => p.Tipo == tipo.Value);

            if (!string.IsNullOrWhiteSpace(ciudad))
            {
                var cLike = $"%{ciudad.Trim()}%";
                query = query.Where(p => p.Direccion != null &&
                                         EF.Functions.Like(p.Direccion.Ciudad ?? "", cLike));
            }

            if (min.HasValue)
                query = query.Where(p => p.PrecioPorNoche.HasValue && p.PrecioPorNoche.Value >= min.Value);

            if (max.HasValue)
                query = query.Where(p => p.PrecioPorNoche.HasValue && p.PrecioPorNoche.Value <= max.Value);

            query = orden switch
            {
                "precio_asc" => query.OrderBy(p => p.PrecioPorNoche ?? decimal.MaxValue),
                "precio_desc" => query.OrderByDescending(p => p.PrecioPorNoche ?? decimal.Zero),
                "titulo" => query.OrderBy(p => p.Titulo),
                "titulo_desc" => query.OrderByDescending(p => p.Titulo),
                _ => query.OrderByDescending(p => p.Id)
            };

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Total = (total == 0 && items.Count > 0) ? items.Count : total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Query = q;
            ViewBag.Tipo = tipo;
            ViewBag.Ciudad = ciudad;
            ViewBag.Min = min;
            ViewBag.Max = max;
            ViewBag.Orden = orden;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            Console.WriteLine($"Buscando propiedad con ID: {id}");

            Propiedad? prop = null;

            // Primero intentar buscar como string (comparando el Id convertido a string)
            if (!string.IsNullOrEmpty(id))
            {
                prop = await _db.Propiedades
                    .Include(p => p.Imagenes)
                    .Include(p => p.Resenas)
                    .ThenInclude(r => r.Usuario)
                    .Include(p => p.Habitaciones) // Incluir habitaciones para hoteles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id.ToString() == id);
            }

            // Si no se encontró como string, intentar como Guid
            if (prop == null && Guid.TryParse(id, out Guid idGuid))
            {
                Console.WriteLine($"Buscando como Guid: {idGuid}");
                prop = await _db.Propiedades
                    .Include(p => p.Imagenes)
                    .Include(p => p.Resenas)
                    .ThenInclude(r => r.Usuario)
                    .Include(p => p.Habitaciones) // Incluir habitaciones para hoteles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == idGuid);
            }

            Console.WriteLine($"Propiedad encontrada: {prop != null}");

            if (prop == null) return NotFound();

            // ✅ Llamada al API del clima
            if (prop.Direccion?.Ciudad != null)
            {
                var clima = await _openWeather.ObtenerClimaAsync(prop.Direccion.Ciudad);
                if (clima != null)
                {
                    ViewBag.Temp = clima.Temperatura;
                    ViewBag.Clima = clima.Descripcion;
                }
                else
                {
                    ViewBag.Temp = "N/D";
                    ViewBag.Clima = "No disponible";
                }
            }

            // 🖼️ Llamada al API de Unsplash
            try
            {
                ViewBag.ImagenUnsplash = !string.IsNullOrWhiteSpace(prop.Titulo)
                    ? await _unsplash.ObtenerImagenAsync(prop.Titulo) ?? ""
                    : "";
            }
            catch
            {
                ViewBag.ImagenUnsplash = "";
            }

            return View(prop);
        }
    }
}