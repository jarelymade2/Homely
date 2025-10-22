using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StayGo.Integration
{
    public class OpenWeatherIntegration
    {
        private readonly IConfiguration _config;
        private readonly ILogger<OpenWeatherIntegration> _logger;

        public OpenWeatherIntegration(IConfiguration config, ILogger<OpenWeatherIntegration> logger)
        {
            _config = config;
            _logger = logger;
        }

        // ✅ Obtiene el clima por ciudad (más simple para usar en tu PropiedadController)
        public async Task<WeatherResult?> ObtenerClimaAsync(string ciudad)
        {
            try
            {
                using var client = new HttpClient();
                var apiKey = _config["OpenWeather:ApiKey"];
                var baseUrl = _config["OpenWeather:ApiUrl"];

                var url = $"{baseUrl}weather?q={Uri.EscapeDataString(ciudad)}&appid={apiKey}&units=metric&lang=es";

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<OpenWeatherResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (data == null) return null;

                return new WeatherResult
                {
                    Ciudad = data.Name,
                    Temperatura = data.Main.Temp,
                    Descripcion = data.Weather.FirstOrDefault()?.Description ?? "Sin datos",
                    Lat = data.Coord.Lat,
                    Lon = data.Coord.Lon
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clima desde OpenWeather");
                return null;
            }
        }
    }

    // 🔹 Modelos para deserializar la respuesta de OpenWeather
    public class OpenWeatherResponse
    {
        public WeatherCoord Coord { get; set; }
        public WeatherMain Main { get; set; }
        public List<WeatherDescription> Weather { get; set; }
        public string Name { get; set; }
    }

    public class WeatherCoord
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class WeatherMain
    {
        public double Temp { get; set; }
        public double Humidity { get; set; }
    }

    public class WeatherDescription
    {
        public string Description { get; set; }
        public string Icon { get; set; }
    }

    // 🔹 Modelo simplificado para la vista/controlador
    public class WeatherResult
    {
        public string Ciudad { get; set; }
        public double Temperatura { get; set; }
        public string Descripcion { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}
