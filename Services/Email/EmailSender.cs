using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace StayGo.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSenderOptions _opt;
        public EmailSender(IOptions<EmailSenderOptions> opt) => _opt = opt.Value;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(_opt.ApiKey))
                throw new InvalidOperationException("Falta SendGrid.ApiKey");

            var client = new SendGridClient(_opt.ApiKey);
            var from   = new EmailAddress(_opt.SenderEmail, _opt.SenderName);
            var to     = new EmailAddress(email);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: htmlMessage);
            var res = await client.SendEmailAsync(msg);

            if ((int)res.StatusCode >= 400)
                throw new Exception($"Error enviando correo: {(int)res.StatusCode}");
        }
    }
}
