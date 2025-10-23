using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StayGo.Integration
{
    public class UnsplashIntegration
    {
        private readonly IConfiguration _config;
        private readonly ILogger<UnsplashIntegration> _logger;

        public UnsplashIntegration(IConfiguration config, ILogger<UnsplashIntegration> logger)
        {
            _config = config;
            _logger = logger;
        }

        // Método para obtener una imagen por palabra clave
        public async Task<string?> ObtenerImagenAsync(string query)
        {
            try
            {
                using var client = new HttpClient();
                var accessKey = _config["Unsplash:AccessKey"];
                var baseUrl = _config["Unsplash:ApiUrl"];

                var url = $"{baseUrl}?query={Uri.EscapeDataString(query)}&client_id={accessKey}&per_page=1";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var results = doc.RootElement.GetProperty("results");
                if (results.GetArrayLength() > 0)
                {
                    return results[0].GetProperty("urls").GetProperty("regular").GetString();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener imagen de Unsplash");
                return null;
            }
        }
    }
}
