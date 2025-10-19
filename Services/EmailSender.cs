using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using StayGo.Services; // Necesario para AuthMessageSenderOptions

namespace StayGo.Services
{
    // Clase que implementa la lógica real de envío de correos usando SendGrid.
    public class EmailSender : IEmailSender
    {
        // Opciones de configuración (ApiKey y usuario) inyectadas desde appsettings.json
        public AuthMessageSenderOptions Options { get; }

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        // Método principal llamado por ASP.NET Identity para enviar un correo
        public Task SendEmailAsync(string toEmail, string subject, string message)
        {
            // Verificación de la clave
            if (string.IsNullOrEmpty(Options.SendGridKey))
            {
                Console.WriteLine("ERROR: SendGridKey no configurada. El correo no se enviará.");
                return Task.CompletedTask; 
            }
            return Execute(Options.SendGridKey, subject, message, toEmail);
        }

        // Lógica de ejecución del envío a través del cliente de SendGrid
        private Task Execute(string apiKey, string subject, string message, string toEmail)
        {
            var client = new SendGridClient(apiKey);
            
            // 🛑 REMITENTE: Cambia este correo por el que VERIFICASTE en tu cuenta de SendGrid
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("tu_correo_verificado@tudominio.com", Options.SendGridUser), 
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message // Contenido HTML del correo (útil para enlaces de restablecimiento)
            };
            msg.AddTo(new EmailAddress(toEmail));

            // Desactivamos el seguimiento de clics.
            msg.SetClickTracking(false, false);

            return client.SendEmailAsync(msg);
        }
    }
}
