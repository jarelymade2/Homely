using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using StayGo.Models; // Asegúrate de que esta sea la ruta correcta a tu ApplicationUser

namespace StayGo.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public EmailModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // **CORRECCIÓN CS8618:** Inicializar para evitar advertencias de nulidad.
        public string Email { get; set; } = string.Empty;

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        // **CORRECCIÓN CS8618:** Inicializar para evitar advertencias de nulidad.
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required(ErrorMessage = "El nuevo email es obligatorio.")]
            [EmailAddress(ErrorMessage = "Formato de email inválido.")]
            [Display(Name = "Nuevo email")]
            // No acepta nulo porque es [Required] y se maneja en el PageModel
            public string NewEmail { get; set; } = string.Empty; 
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email ?? throw new InvalidOperationException($"No se pudo cargar el email para el usuario con ID '{_userManager.GetUserId(User)}'.");

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
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

        public async Task<IActionResult> OnPostChangeEmailAsync()
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

            var email = await _userManager.GetEmailAsync(user);
            
            // Comprobamos si el nuevo email es diferente al actual
            if (Input.NewEmail != email)
            {
                // Generamos el token de cambio de email
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                
                // Codificamos el ID y el código para la URL
                var codeEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = codeEncoded },
                    protocol: Request.Scheme);

                // Enviamos el correo de confirmación
                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    "Confirma tu cambio de email en Homely",
                    $"Por favor, confirma tu cuenta haciendo <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>click aquí</a>.");

                StatusMessage = "Se ha enviado un enlace de confirmación para el cambio de email. Revisa tu correo.";
                return RedirectToPage();
            }

            StatusMessage = "Tu email no ha sido cambiado ya que es el mismo que el actual.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
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

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            
            // Generamos el token de verificación de email
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // Codificamos el ID y el código para la URL
            var codeEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = codeEncoded },
                protocol: Request.Scheme);

            // Enviamos el correo de verificación
            await _emailSender.SendEmailAsync(
                email!,
                "Confirma tu email en Homely",
                $"Por favor, confirma tu cuenta haciendo <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>click aquí</a>.");

            StatusMessage = "Email de verificación enviado. Revisa tu correo.";
            return RedirectToPage();
        }
    }
}