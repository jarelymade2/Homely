using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StayGo.Data;
<<<<<<< HEAD
using StayGo.Models;
=======
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Options;
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952

namespace StayGo.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly StayGoContext _context;
<<<<<<< HEAD
        private readonly ILogger<RegisterModel> _logger;
=======
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GoogleReCaptchaSettings _captchaSettings;
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            StayGoContext context,
<<<<<<< HEAD
            ILogger<RegisterModel> logger)
=======
            IHttpClientFactory httpClientFactory,
            IOptions<GoogleReCaptchaSettings> captchaSettings)
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
        {
            _userManager   = userManager;
            _signInManager = signInManager;
<<<<<<< HEAD
            _context       = context;
            _logger        = logger;
=======
            _context = context;
            _httpClientFactory = httpClientFactory;
            _captchaSettings = captchaSettings.Value;
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        // ✅ Captura el token de reCAPTCHA y es obligatorio
        [BindProperty]
        [Required(ErrorMessage = "Debes completar el reCAPTCHA.")]
        public string Recaptcha { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }

        // <-- NECESARIO para el botón de Google en la vista
        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        public class InputModel
        {
<<<<<<< HEAD
            [Required] public string FirstName { get; set; } = "";
            [Required] public string LastName  { get; set; } = "";

            [Required, EmailAddress]
            public string Email { get; set; } = "";

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
=======
            [Required(ErrorMessage = "El nombre es obligatorio.")]
            public string FirstName { get; set; } = "";

            [Required(ErrorMessage = "El apellido es obligatorio.")]
            public string LastName { get; set; } = "";

            [Required(ErrorMessage = "El correo es obligatorio.")]
            [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Required(ErrorMessage = "Confirme la contraseña.")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
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
<<<<<<< HEAD
            ReturnUrl = returnUrl ?? Url.Content("~/");
            // Si hay error y se vuelve a mostrar la página, esto debe estar cargado
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
=======
            returnUrl ??= Url.Content("~/");

            // Validar reCAPTCHA
            if (!await ValidarReCaptchaAsync())
            {
                ModelState.AddModelError(string.Empty, "Por favor completa el reCAPTCHA antes de continuar.");
                return Page();
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952

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
<<<<<<< HEAD
                        Id             = Guid.NewGuid(),
                        IdentityUserId = user.Id,
                        Email          = user.Email!,
                        EsAdmin        = false
=======
                        Id = Guid.NewGuid(),
                        IdentityUserId = user.Id,
                        Email = user.Email,
                        EsAdmin = false
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
                    };

                    _context.Usuarios.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();
<<<<<<< HEAD
                }
                catch (Exception ex)
=======

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
                {
                    _logger.LogWarning(ex, "No se pudo crear fila en Usuario para {UserId}", user.Id);
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(ReturnUrl!);
            }

<<<<<<< HEAD
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
=======
            // Si llegamos aquí, hubo un error
            return Page();
        }

        private async Task<bool> ValidarReCaptchaAsync()
        {
            var recaptchaResponse = Request.Form["g-recaptcha-response"].ToString();

            if (string.IsNullOrEmpty(recaptchaResponse))
                return false;

            var client = _httpClientFactory.CreateClient();
            var secret = _captchaSettings.SecretKey;

            var response = await client.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={recaptchaResponse}",
                null);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ReCaptchaResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Success ?? false;
        }

        private class ReCaptchaResponse
        {
            public bool Success { get; set; }
            public DateTime Challenge_ts { get; set; }
            public string Hostname { get; set; } = "";
            public string[] ErrorCodes { get; set; } = Array.Empty<string>();
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
        }
    }
}
