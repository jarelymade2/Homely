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

                // Nota: Usamos el operador de nulabilidad (?) y el operador null-coalescing (??) 
                // para evitar advertencias y posibles NullReferenceExceptions al acceder a los datos.
                return new WeatherResult
                {
                    Ciudad = data.Name ?? "N/A",
                    Temperatura = data.Main?.Temp ?? 0, // Añadido ? para Main
                    Descripcion = data.Weather?.FirstOrDefault()?.Description ?? "Sin datos", // Añadido ? para Weather
                    Lat = data.Coord?.Lat ?? 0, // Añadido ? para Coord
                    Lon = data.Coord?.Lon ?? 0  // Añadido ? para Coord
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
        // 🛑 Corrección CS8618: Añadir ?
        public WeatherCoord? Coord { get; set; } // Línea 61
        public WeatherMain? Main { get; set; } // Línea 62
        public List<WeatherDescription>? Weather { get; set; } // Línea 63
        public string? Name { get; set; } // Línea 64
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
        // 🛑 Corrección CS8618: Añadir ?
        public string? Description { get; set; } // Línea 81
        public string? Icon { get; set; } // Línea 82
    }

    // 🔹 Modelo simplificado para la vista/controlador
    public class WeatherResult
    {
        // 🛑 Corrección CS8618: Añadir ?
        public string? Ciudad { get; set; } // Línea 88
        public double Temperatura { get; set; } // double no es tipo de referencia, no necesita ?
        public string? Descripcion { get; set; } // Línea 90
        public double Lat { get; set; } // double no necesita ?
        public double Lon { get; set; } // double no necesita ?
    }
}