using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums;

namespace StayGo.Controllers;

public class PropiedadController : Controller
{
    private readonly StayGoContext _db;
    public PropiedadController(StayGoContext db) => _db = db;

    // GET: /Propiedad
    public async Task<IActionResult> Index(string? q, TipoPropiedad? tipo, string? ciudad,
                                           decimal? min, decimal? max,
                                           string? orden = "recientes",
                                           int page = 1, int pageSize = 9)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 60) pageSize = 9;

        // Base query
        IQueryable<Propiedad> query = _db.Propiedades
            .AsNoTracking()
            .Include(p => p.Imagenes);

        // FILTRO búsqueda libre
        if (!string.IsNullOrWhiteSpace(q))
        {
            var qNorm = q.Trim().ToLowerInvariant();
            query = query.Where(p =>
                p.Titulo.ToLower().Contains(qNorm) ||
                (p.Descripcion != null && p.Descripcion.ToLower().Contains(qNorm)) ||
                (p.Direccion != null && (
                    (p.Direccion.Ciudad ?? "").ToLower().Contains(qNorm) ||
                    (p.Direccion.Pais ?? "").ToLower().Contains(qNorm) ||
                    (p.Direccion.Linea1 ?? "").ToLower().Contains(qNorm) ||
                    (p.Direccion.Linea2 ?? "").ToLower().Contains(qNorm) ||
                    (p.Direccion.CodigoPostal ?? "").ToLower().Contains(qNorm)
                ))
            );
        }

        // FILTRO tipo
        if (tipo.HasValue)
            query = query.Where(p => p.Tipo == tipo);

        // FILTRO ciudad
        if (!string.IsNullOrWhiteSpace(ciudad))
        {
            var c = ciudad.Trim().ToLowerInvariant();
            query = query.Where(p => p.Direccion != null &&
                                     (p.Direccion.Ciudad ?? "").ToLower().Contains(c));
        }

        // FILTRO precios
        if (min.HasValue)
            query = query.Where(p => p.PrecioPorNoche.HasValue && p.PrecioPorNoche.Value >= min.Value);

        if (max.HasValue)
            query = query.Where(p => p.PrecioPorNoche.HasValue && p.PrecioPorNoche.Value <= max.Value);

        // ORDEN
        query = orden switch
        {
            "precio_asc" => query.OrderBy(p => p.PrecioPorNoche ?? decimal.MaxValue),
            "precio_desc" => query.OrderByDescending(p => p.PrecioPorNoche ?? decimal.Zero),
            "titulo" => query.OrderBy(p => p.Titulo),
            _ => query.OrderByDescending(p => p.Id) // recientes
        };

        // PAGINACIÓN
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Demo en memoria si la BD está vacía
        if (total == 0 && items.Count == 0)
        {
            items = new List<Propiedad>
            {
                new Propiedad {
                    Titulo = "Casa de Playa",
                    Direccion = new Models.ValueObjects.Direccion {
                        Ciudad = "Lima", Pais = "Perú", Linea1 = "Costa Verde"
                    },
                    PrecioPorNoche = 200m
                },
                new Propiedad {
                    Titulo = "Departamento céntrico",
                    Direccion = new Models.ValueObjects.Direccion {
                        Ciudad = "Cusco", Pais = "Perú", Linea1 = "Av. El Sol 123"
                    },
                    PrecioPorNoche = 150m
                },
                new Propiedad {
                    Titulo = "Cabaña en la montaña",
                    Direccion = new Models.ValueObjects.Direccion {
                        Ciudad = "Arequipa", Pais = "Perú", Linea1 = "Valle de Chilina"
                    },
                    PrecioPorNoche = 120m
                }
            };
        }

        // Metadatos para la vista
        ViewBag.Total = total;
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

    // GET: /Propiedad/Details/{id}
    public async Task<IActionResult> Details(Guid id)
    {
        var prop = await _db.Propiedades
            .Include(p => p.Imagenes)
            .Include(p => p.Resenas)
            .Include(p => p.Disponibilidades)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prop == null) return NotFound();

        return View(prop);
    }
    // GET: /Propiedad/Crear
[HttpGet]
public IActionResult Crear()
{
    return View(new Propiedad { Tipo = TipoPropiedad.Casa, Capacidad = 1 });
}

// POST: /Propiedad/Crear
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Crear(Propiedad model)
{
    if (!ModelState.IsValid)
        return View(model); // 👈 Aquí verás los errores de FluentValidation

    _db.Add(model);
    await _db.SaveChangesAsync();
    TempData["ok"] = "Propiedad creada";
    return RedirectToAction(nameof(Index));
}

}
