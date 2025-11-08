namespace StayGo.Models.ValueObjects;

// Esta clase mapea la sección "SendGrid" del archivo appsettings.json.
public class SendGridOptions
{
    // Clave API de SendGrid
    public string ApiKey { get; set; } = string.Empty;

    // Correo desde donde se enviarán los mensajes (ej: no-reply@homely.com)
    public string SenderEmail { get; set; } = string.Empty;

    // Nombre que aparecerá como remitente (ej: Equipo HomeLy)
    public string SenderName { get; set; } = string.Empty;
}