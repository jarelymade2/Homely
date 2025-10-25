using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StayGo.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks; // Añadido para asegurar que Task se reconoce

namespace StayGo.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // CORRECCIÓN CS8618: Inicializar las propiedades
        public string Username { get; set; } = string.Empty;

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel(); 

        public class InputModel
        {
            [Phone]
            [Display(Name = "Número de teléfono")]
            public string? PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Nombre")]
            // NOTA: Si pones [Required], puedes usar 'string' en lugar de 'string?' 
            // pero si usas 'string?', debes ser cuidadoso donde lo asignas. 
            // Lo dejamos como 'string?' ya que así está en tu código.
            public string? FirstName { get; set; }

            [Required]
            [Display(Name = "Apellido")]
            public string? LastName { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            // CORRECCIÓN CS8601 (Línea 50): Usamos '!' para asegurar que userName no es nulo aquí
            Username = userName!;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // --- Lógica de actualización de campos ---

            // 1. Actualizar Nombre y Apellido
            // CORRECCIÓN CS8601 (Líneas 91 y 95): Usamos '!' en la asignación para suprimir la advertencia,
            // ya que ModelState.IsValid pasó (y ambos tienen [Required]).
            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName!; 
            }
            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName!;
            }
            
            // 2. Actualizar Teléfono
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Error inesperado al intentar actualizar el número de teléfono.";
                    return RedirectToPage();
                }
            }
            
            // 3. Guardar cambios en el usuario (incluyendo FirstName y LastName)
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Tu perfil ha sido actualizado";
            return RedirectToPage();
        }
    }
}