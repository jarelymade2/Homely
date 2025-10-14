using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging; // Añadir si no está
using System.ComponentModel.DataAnnotations;
using StayGo.Models; // Necesario para ApplicationUser

namespace StayGo.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        // Campos de Identity necesarios
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager; // <-- AÑADIDO: Para verificación de rol
        private readonly ILogger<LoginModel> _logger; // Opcional pero recomendado para logs

        // Constructor modificado para inyectar UserManager
        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, // <-- AÑADIDO
            ILogger<LoginModel> logger) // Opcional
        {
            _signInManager = signInManager;
            _userManager = userManager; // <-- Asignación
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El email es obligatorio.")]
            [EmailAddress(ErrorMessage = "El email no es válido.")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Intenta iniciar sesión con el email y la contraseña
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // ----------------------------------------------------
                // LÓGICA DE REDIRECCIÓN CONDICIONAL BASADA EN ROL
                // ----------------------------------------------------

                // 1. Encontrar al usuario
                var user = await _userManager.FindByEmailAsync(Input.Email);

                // 2. Verificar el rol "Admin"
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    _logger.LogInformation("Usuario Admin ha iniciado sesión.");
                    // Redirigir al área de Administración
                    return LocalRedirect("/Admin/Admin/Index"); 
                }
                
                // Si el usuario no es Admin, o si es un usuario normal
                _logger.LogInformation("Usuario ha iniciado sesión.");
                return LocalRedirect(ReturnUrl);
            }

            // Si falla el inicio de sesión
            ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido.");
            return Page();
        }
    }
}