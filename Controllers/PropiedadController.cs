using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums;
using Microsoft.AspNetCore.Identity; // NECESARIO
using System.Text.Json; // NECESARIO
using System.Threading.Tasks; // NECESARIO

namespace StayGo.Controllers;

public class PropiedadController : Controller
{
    private readonly StayGoContext _db;
    private readonly UserManager<ApplicationUser> _userManager; // Nuevo: Campo para Identity

    // Constructor actualizado para inyectar UserManager
    public PropiedadController(StayGoContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: /Propiedad
    [HttpGet]
    public async Task<IActionResult> Index( // Asegúrate de que es async Task<IActionResult>
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

        // --- LÓGICA DE HISTORIAL DE BÚSQUEDA PERSISTENTE ---
        List<string> historial = new List<string>();

        if (User.Identity!.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                // 1. Cargar historial
                historial = JsonSerializer.Deserialize<List<string>>(user.SearchHistoryJson) ?? new List<string>();

                // 2. Si hay una búsqueda 'q', actualizar el historial y guardar
                if (!string.IsNullOrWhiteSpace(q))
                {
                    var currentQuery = q.Trim();

                    // Limpiar y añadir al inicio (la búsqueda más reciente)
                    historial.Remove(currentQuery);
                    historial.Insert(0, currentQuery);

                    // Limitar a 5 elementos
                    if (historial.Count > 5)
                    {
                        historial.RemoveRange(5, historial.Count - 5);
                    }

                    // Guardar en la DB
                    user.SearchHistoryJson = JsonSerializer.Serialize(historial);
                    await _userManager.UpdateAsync(user); 
                }
            }
        }
        
        // Pasa el historial a la vista
        ViewBag.HistorialBusqueda = historial;
        // --- FIN LÓGICA DE HISTORIAL ---

        // Base query (código existente)
        IQueryable<Propiedad> query = _db.Propiedades
            .Include(p => p.Imagenes)
            .AsNoTracking();

        // FILTRO: búsqueda libre (código existente)
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

        // ... El resto de tus filtros y orden (sin cambios) ...
        
        // FILTRO: tipo
        if (tipo.HasValue)
            query = query.Where(p => p.Tipo == tipo.Value);

        // FILTRO: ciudad
        if (!string.IsNullOrWhiteSpace(ciudad))
        {
            var cLike = $"%{ciudad.Trim()}%";
            query = query.Where(p => p.Direccion != null &&
                                     EF.Functions.Like(p.Direccion.Ciudad ?? "", cLike));
        }

        // FILTRO: precios
        if (min.HasValue)
            query = query.Where(p => p.PrecioPorNoche.HasValue && p.PrecioPorNoche.Value >= min.Value);

        if (max.HasValue)
            query = query.Where(p => p.PrecioPorNoche.HasValue && p.PrecioPorNoche.Value <= max.Value);

        // ORDEN
        query = orden switch
        {
            "precio_asc"  => query.OrderBy(p => p.PrecioPorNoche ?? decimal.MaxValue),
            "precio_desc" => query.OrderByDescending(p => p.PrecioPorNoche ?? decimal.Zero),
            "titulo"      => query.OrderBy(p => p.Titulo),
            "titulo_desc" => query.OrderByDescending(p => p.Titulo),
            _             => query.OrderByDescending(p => p.Id) // recientes (proxy)
        };

        // PAGINACIÓN (código existente)
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Demo en memoria si la BD está vacía (código existente)
        if (total == 0 && items.Count == 0)
        {
             // ... [Demo items] ...
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

        // Metadatos para la vista (código existente)
        var totalParaVista = (total == 0 && items.Count > 0) ? items.Count : total;
        ViewBag.Total = totalParaVista;
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

    // GET: /Propiedad/Details/{id} (código existente)
    [HttpGet]
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
}