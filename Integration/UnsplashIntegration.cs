using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StayGo.Services;

namespace StayGo.Integration
{
    public class UnsplashIntegration
    {
        private readonly IConfiguration _config;
        private readonly ILogger<UnsplashIntegration> _logger;
        private readonly ICacheService _cache;

        public UnsplashIntegration(IConfiguration config, ILogger<UnsplashIntegration> logger, ICacheService cache)
        {
            _config = config;
            _logger = logger;
            _cache = cache;
        }

        // Método para obtener una imagen por palabra clave (con caché)
        public async Task<string?> ObtenerImagenAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            // Crear clave de caché para la búsqueda
            var cacheKey = $"unsplash:{query.ToLowerInvariant()}";
            var cacheExpiration = TimeSpan.FromMinutes(
                _config.GetValue<int>("Redis:CacheExpirationMinutes:Imagenes", 1440)); // 24 horas por defecto

            // Intentar obtener del caché o crear
            return await _cache.GetOrCreateAsync(cacheKey, async () =>
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
                        return results[0].GetProperty("urls").GetProperty("regular").GetString() ?? string.Empty;
                    }

                    return string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener imagen de Unsplash");
                    return string.Empty;
                }
            }, cacheExpiration);
        }
    }
}
