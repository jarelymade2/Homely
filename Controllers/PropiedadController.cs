using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;

namespace StayGo.Controllers;
public class PropiedadController : Controller
{
    private readonly StayGoContext _db;
    public PropiedadController(StayGoContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
         var modelo = await _db.Propiedades.ToListAsync();

            if (!modelo.Any())
            {
                modelo = new List<Propiedad>
                {
                    new Propiedad { Nombre = "Casa de Playa", Ubicación = "Lima", PrecioPorNoche = 200 },
                    new Propiedad { Nombre = "Departamento céntrico", Ubicación = "Cusco", PrecioPorNoche = 150 },
                    new Propiedad { Nombre = "Cabaña en la montaña", Ubicación = "Arequipa", PrecioPorNoche = 120 }
                };
            }

            return View(modelo);
    }
}
