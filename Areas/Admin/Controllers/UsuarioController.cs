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
            // 🛑 PASO 1: OBTENER EL ID DEL ROL 'Admin' DE FORMA SEGURA
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            
            // Si el rol Admin no existe, devolvemos una lista vacía o manejamos el error.
            if (adminRole == null)
            {
                // Podrías registrar un error aquí, pero devolver una lista vacía es seguro.
                return View(new List<UsuarioAdminViewModel>());
            }
            
            // Guardamos el ID del rol para usarlo en la consulta
            var adminRoleId = adminRole.Id;

            // 🛑 PASO 2: USAR EL ID DEL ROL EN LA CONSULTA LINQ
            var listaUsuarios = await _context.Users
                .OrderBy(u => u.LastName)
                .Select(u => new UsuarioAdminViewModel
                {
                    Id = u.Id,
                    NombreCompleto = (u.FirstName ?? "") + " " + (u.LastName ?? ""),
                    Email = u.Email ?? "N/A",
                    // Asumiendo que 'Reservas' es una colección de navegación en ApplicationUser
                    TotalReservas = u.Reservas != null ? u.Reservas.Count() : 0, 
                    
                    // La consulta se simplifica y es segura porque adminRoleId es una variable local no nula.
                    EsAdmin = _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId)
                })
                .ToListAsync();

            return View(listaUsuarios);
        }
    }
}