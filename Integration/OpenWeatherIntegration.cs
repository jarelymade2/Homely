using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace StayGo.Integration
{
    public class OpenWeatherIntegration
    {
        private readonly HttpClient _http;
        private readonly string _apiUrl;
        private readonly string _apiKey;

        public OpenWeatherIntegration(HttpClient http, IConfiguration configuration)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _apiUrl = configuration.GetValue<string>("OpenWeather:ApiUrl")?.TrimEnd('/') + "/" ?? "https://api.openweathermap.org/data/2.5/";
            _apiKey = configuration.GetValue<string>("OpenWeather:ApiKey") ?? string.Empty;
        }

        /// <summary>
        /// Obtiene la información del clima para una ciudad (en unidades métricas).
        /// </summary>
        public async Task<WeatherInfo?> GetWeatherAsync(string ciudad)
        {
            if (string.IsNullOrWhiteSpace(ciudad)) return null;

            var url = $"{_apiUrl}weather?q={Uri.EscapeDataString(ciudad)}&appid={_apiKey}&units=metric&lang=es";
            using var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResp = JsonSerializer.Deserialize<OpenWeatherResponse>(json, opts);
            if (apiResp == null) return null;

            return new WeatherInfo
            {
                Coord = apiResp.Coord ?? new Coord(),
                Main = apiResp.Main ?? new Main(),
                Weather = apiResp.Weather ?? new List<Weather>(),
                Name = apiResp.Name ?? string.Empty
            };
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
        // ...existing code...
        // Wrapper con nombre en español usado por el controlador
        public async Task<ClimaDto?> ObtenerClimaAsync(string ciudad)
        {
            var info = await GetWeatherAsync(ciudad);
            if (info == null) return null;

            var temperatura = info.Main?.Temp ?? 0.0;
            var descripcion = info.Weather != null && info.Weather.Count > 0
                ? info.Weather[0].Description
                : string.Empty;

            return new ClimaDto
            {
                Temperatura = temperatura,
                Descripcion = descripcion
            };
        }

        // DTO simple que espera el controlador (Temperatura, Descripcion)
        public class ClimaDto
        {
            public double Temperatura { get; set; }
            public string Descripcion { get; set; } = string.Empty;
        }
    // ...existing code...
    }

    // DTOs para deserializar la respuesta de OpenWeather
    public class OpenWeatherResponse
    {
<<<<<<< HEAD
        public Coord? Coord { get; set; }
        public Main? Main { get; set; }
        public List<Weather>? Weather { get; set; }
        public string? Name { get; set; }
=======
        // 🛑 Corrección CS8618: Añadir ?
        public WeatherCoord? Coord { get; set; } // Línea 61
        public WeatherMain? Main { get; set; } // Línea 62
        public List<WeatherDescription>? Weather { get; set; } // Línea 63
        public string? Name { get; set; } // Línea 64
>>>>>>> 08d0f9a3ae50d718c34c0420da9cd8bac3c0e0e3
    }

    public class Coord
    {
        public double Lon { get; set; }
        public double Lat { get; set; }
    }

    public class Main
    {
        public double Temp { get; set; }
        public int Humidity { get; set; }
        public double? Temp_Min { get; set; }
        public double? Temp_Max { get; set; }
    }

    public class Weather
    {
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int? Id { get; set; }
        public string? Main { get; set; }
    }

    // Clase simplificada que tu aplicación puede usar (mapeo amigable)
    public class WeatherInfo
    {
        public Coord Coord { get; set; } = new Coord();
        public Main Main { get; set; } = new Main();
        public List<Weather> Weather { get; set; } = new List<Weather>();
        public string Name { get; set; } = string.Empty;
    }

    // Clase para representar una descripción breve (si la necesitas)
    public class WeatherDescription
    {
<<<<<<< HEAD
        public string Ciudad { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
=======
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
>>>>>>> 08d0f9a3ae50d718c34c0420da9cd8bac3c0e0e3
}