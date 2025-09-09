using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models; // donde estén Reserva y Propiedad

namespace StayGo.Controllers
{
    public class ReservasController : Controller
    {
        private readonly StayGoContext _db;
        public ReservasController(StayGoContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var reservas = await _db.Reservas
                .Include(r => r.Propiedad)
                .AsNoTracking()
                .ToListAsync();

            return View(reservas);
        }
    }
}
