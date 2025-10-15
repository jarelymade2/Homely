using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums;
using StayGo.ViewModels.Admin;

namespace StayGo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReservaController : Controller
    {
        private readonly StayGoContext _context;

        public ReservaController(StayGoContext context)
        {
            _context = context;
        }

        // GET: Admin/Reserva
        public async Task<IActionResult> Index()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Propiedad)
                .Include(r => r.Usuario)
                .OrderByDescending(r => r.CheckIn)
                .ToListAsync();
            return View(reservas);
        }

        // GET: Admin/Reserva/Create
        public async Task<IActionResult> Crear()
        {
            var viewModel = new ReservaViewModel
            {
                Propiedades = await GetPropiedadesSelectList(),
                Usuarios = await GetUsuariosSelectList()
            };
            return View(viewModel);
        }

        // POST: Admin/Reserva/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ReservaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var reserva = new Reserva
                {
                    Id = Guid.NewGuid(),
                    PropiedadId = viewModel.PropiedadId,
                    HabitacionId = viewModel.HabitacionId,
                    UsuarioId = viewModel.UsuarioId,
                    CheckIn = DateOnly.FromDateTime(viewModel.CheckIn),
                    CheckOut = DateOnly.FromDateTime(viewModel.CheckOut),
                    Huespedes = viewModel.Huespedes,
                    PrecioTotal = viewModel.PrecioTotal,
                    Estado = viewModel.Estado
                };
                
                _context.Add(reserva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            viewModel.Propiedades = await GetPropiedadesSelectList();
            viewModel.Usuarios = await GetUsuariosSelectList();
            return View(viewModel);
        }

        // GET: Admin/Reserva/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return NotFound();

            var viewModel = new ReservaViewModel
            {
                Id = reserva.Id,
                PropiedadId = reserva.PropiedadId,
                HabitacionId = reserva.HabitacionId,
                UsuarioId = reserva.UsuarioId,
                CheckIn = reserva.CheckIn.ToDateTime(TimeOnly.MinValue),
                CheckOut = reserva.CheckOut.ToDateTime(TimeOnly.MinValue),
                Huespedes = reserva.Huespedes,
                PrecioTotal = reserva.PrecioTotal,
                Estado = reserva.Estado,
                Propiedades = await GetPropiedadesSelectList(),
                Usuarios = await GetUsuariosSelectList()
            };
            
            return View(viewModel);
        }

        // POST: Admin/Reserva/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ReservaViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var reservaToUpdate = await _context.Reservas.FindAsync(id);
                if (reservaToUpdate == null) return NotFound();
                
                reservaToUpdate.PropiedadId = viewModel.PropiedadId;
                reservaToUpdate.HabitacionId = viewModel.HabitacionId;
                reservaToUpdate.UsuarioId = viewModel.UsuarioId;
                reservaToUpdate.CheckIn = DateOnly.FromDateTime(viewModel.CheckIn);
                reservaToUpdate.CheckOut = DateOnly.FromDateTime(viewModel.CheckOut);
                reservaToUpdate.Huespedes = viewModel.Huespedes;
                reservaToUpdate.PrecioTotal = viewModel.PrecioTotal;
                reservaToUpdate.Estado = viewModel.Estado;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            viewModel.Propiedades = await GetPropiedadesSelectList();
            viewModel.Usuarios = await GetUsuariosSelectList();
            return View(viewModel);
        }

        // GET: Admin/Reserva/Cancel/5
        public async Task<IActionResult> Cancel(Guid? id)
        {
            if (id == null) return NotFound();
            var reserva = await _context.Reservas
                .Include(r => r.Propiedad)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reserva == null) return NotFound();
            return View(reserva);
        }

        // POST: Admin/Reserva/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(Guid id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                reserva.Estado = EstadoReserva.Cancelada;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        
        // --- Métodos privados para llenar los SelectLists ---
        private async Task<IEnumerable<SelectListItem>> GetPropiedadesSelectList()
        {
            return await _context.Propiedades
                .OrderBy(p => p.Titulo)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Titulo })
                .ToListAsync();
        }
    
        private async Task<IEnumerable<SelectListItem>> GetUsuariosSelectList()
        {
            return await _context.Users
                .OrderBy(u => u.Email)
                .Select(u => new SelectListItem { Value = u.Id, Text = u.Email })
                .ToListAsync();
        }
    }
}