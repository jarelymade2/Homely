using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StayGo.Data;
using StayGo.Models;
using Microsoft.EntityFrameworkCore; // <-- 1. Importar Entity Framework
using System.Threading.Tasks;
using StayGo.Models.Enums;

[Area("Admin")]
public class AdminController : Controller
{
    private readonly StayGoContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(StayGoContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // Endpoint para recrear el admin (sin autorización para poder usarlo en Render)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> InitializeAdmin()
    {
        try
        {
            // 1. Crear roles si no existen
            string[] roleNames = { "Admin", "User", "Guest" };
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Crear usuario Admin
            const string adminEmail = "admin@staygo.com";
            const string adminPassword = "password123!";

            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "StayGo",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    return Ok(new { message = "✅ Admin user created successfully", email = adminEmail, password = adminPassword });
                }
                else
                {
                    return BadRequest(new { message = "❌ Failed to create admin user", errors = result.Errors.Select(e => e.Description) });
                }
            }
            else
            {
                return Ok(new { message = "⚠️ Admin user already exists", email = adminEmail });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "❌ Error initializing admin", error = ex.Message });
        }
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Index() // 4. Convertir a async Task
    {
        // 5. Definir la fecha de "hoy"
        // Usamos DateOnly porque tu modelo Reserva parece usarlo
        var today = DateOnly.FromDateTime(DateTime.Today);

        // 6. Consultar la base de datos real
        int totalPropiedades = await _context.Propiedades.CountAsync();

        int reservasHoy = await _context.Reservas
            .Where(r => r.CheckIn == today && r.Estado == EstadoReserva.Confirmada) // Solo reservas confirmadas para hoy
            .CountAsync();

        decimal ingresosHoy = await _context.Reservas
            .Where(r => r.CheckIn == today && r.Estado == EstadoReserva.Confirmada)
            .SumAsync(r => r.PrecioTotal);

        int usuariosActivos = await _context.Usuarios.CountAsync(); // Total de usuarios en tu tabla 'Usuario'

        // 7. Usar los nombres CORRECTOS que la vista espera
        ViewBag.TotalPropiedades = totalPropiedades;
        ViewBag.ReservasHoy = reservasHoy;
        ViewBag.UsuariosActivos = usuariosActivos;
        ViewBag.IngresosHoy = ingresosHoy.ToString("N2"); // Formatear como 12345.00

        return View();
    }
}
