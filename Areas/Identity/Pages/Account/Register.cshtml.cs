using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StayGo.Data;
using StayGo.Models;

namespace StayGo.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly StayGoContext _context;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            StayGoContext context,
            ILogger<RegisterModel> logger)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _context       = context;
            _logger        = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        // <-- NECESARIO para el botón de Google en la vista
        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        public class InputModel
        {
            [Required] public string FirstName { get; set; } = "";
            [Required] public string LastName  { get; set; } = "";

            [Required, EmailAddress]
            public string Email { get; set; } = "";

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; } = "";
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            // Carga proveedores externos (Google)
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            // Si hay error y se vuelve a mostrar la página, esto debe estar cargado
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
                return Page();

            var user = new ApplicationUser
            {
                UserName  = Input.Email,
                Email     = Input.Email,
                FirstName = Input.FirstName,
                LastName  = Input.LastName
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // Inserta fila vinculada en tu tabla Usuario
                try
                {
                    var nuevoUsuario = new Usuario
                    {
                        Id             = Guid.NewGuid(),
                        IdentityUserId = user.Id,
                        Email          = user.Email!,
                        EsAdmin        = false
                    };
                    _context.Usuarios.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo crear fila en Usuario para {UserId}", user.Id);
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(ReturnUrl!);
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}
