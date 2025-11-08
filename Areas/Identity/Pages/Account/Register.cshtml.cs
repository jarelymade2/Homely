using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using StayGo.Models;
using StayGo.Data;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace StayGo.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly StayGoContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GoogleReCaptchaSettings _captchaSettings;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            StayGoContext context,
            IHttpClientFactory httpClientFactory,
            IOptions<GoogleReCaptchaSettings> captchaSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _httpClientFactory = httpClientFactory;
            _captchaSettings = captchaSettings.Value;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        // ✅ Captura el token de reCAPTCHA y es obligatorio
        [BindProperty]
        [Required(ErrorMessage = "Debes completar el reCAPTCHA.")]
        public string Recaptcha { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
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
            public string ConfirmPassword { get; set; } = "";
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
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

                if (result.Succeeded)
                {
                    var nuevoUsuario = new Usuario
                    {
                        Id = Guid.NewGuid(),
                        IdentityUserId = user.Id,
                        Email = user.Email,
                        EsAdmin = false
                    };

                    _context.Usuarios.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

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
        }
    }
}
