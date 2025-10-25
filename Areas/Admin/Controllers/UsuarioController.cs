using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.ViewModels.Admin; 
using System.Linq;
using System.Threading.Tasks;

namespace StayGo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsuarioController : Controller
    {
        private readonly StayGoContext _context;

        public UsuarioController(StayGoContext context)
        {
            _context = context;
        }

        // GET: Admin/Usuario
     public async Task<IActionResult> Index()
{
    var listaUsuarios = await _context.Users
        .OrderBy(u => u.LastName)
        .Select(u => new UsuarioAdminViewModel
        {
            Id = u.Id,
            NombreCompleto = (u.FirstName ?? "") + " " + (u.LastName ?? ""),
            Email = u.Email ?? "N/A",
            // Si ApplicationUser tiene una colección de Reservas
            TotalReservas = u.Reservas.Count(),
            EsAdmin = _context.UserRoles.Any(ur => ur.UserId == u.Id && 
                      ur.RoleId == _context.Roles.FirstOrDefault(r => r.Name == "Admin").Id)
        })
        .ToListAsync();

    return View(listaUsuarios);
}
    }
}