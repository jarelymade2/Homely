using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;

namespace StayGo.Controllers;
public class PropiedadesController : Controller
{
    private readonly StayGoContext _db;
    public PropiedadesController(StayGoContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var props = await _db.Propiedades.AsNoTracking().ToListAsync();
        return View(props);
    }
}
