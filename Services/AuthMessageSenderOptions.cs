namespace StayGo.Services
{
    public class AuthMessageSenderOptions
    {
        // Se llena automáticamente desde la sección "SendGridOptions" en appsettings.json
        public string? SendGridKey { get; set; }
        public string? SendGridUser { get; set; } = "StayGoSupport";
    }
}
