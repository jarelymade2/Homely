using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using System.Security.Claims;

namespace StayGo.Areas.Identity.Pages.Account.Manage
{
    public class FavoritosModel : PageModel
    {
        private readonly StayGoContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoritosModel(StayGoContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IList<Propiedad> Favoritos { get; set; } = new List<Propiedad>();
        public string StatusMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            // Obtener los favoritos del usuario
            Favoritos = await _db.Favoritos
                .Where(f => f.UsuarioId == user.Id)
                .Include(f => f.Propiedad)
                .ThenInclude(p => p.Imagenes)
                .Select(f => f.Propiedad)
                .AsNoTracking()
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveFavoriteAsync(Guid propiedadId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var favorito = await _db.Favoritos
                .FirstOrDefaultAsync(f => f.UsuarioId == user.Id && f.PropiedadId == propiedadId);

            if (favorito != null)
            {
                _db.Favoritos.Remove(favorito);
                await _db.SaveChangesAsync();
                StatusMessage = "Propiedad removida de favoritos.";
            }

            return RedirectToPage();
        }
    }
}

