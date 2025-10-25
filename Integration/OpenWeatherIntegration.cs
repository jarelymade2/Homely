using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StayGo.Services;

namespace StayGo.Integration
{
    public class OpenWeatherIntegration
    {
        private readonly HttpClient _http;
        private readonly string _apiUrl;
        private readonly string _apiKey;
        private readonly ICacheService _cache;
        private readonly IConfiguration _configuration;

        public OpenWeatherIntegration(HttpClient http, IConfiguration configuration, ICacheService cache)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _apiUrl = configuration.GetValue<string>("OpenWeather:ApiUrl")?.TrimEnd('/') + "/" ?? "https://api.openweathermap.org/data/2.5/";
            _apiKey = configuration.GetValue<string>("OpenWeather:ApiKey") ?? string.Empty;
            _cache = cache;
            _configuration = configuration;
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
        }

        // Wrapper con nombre en español usado por el controlador (con caché)
        public async Task<ClimaDto?> ObtenerClimaAsync(string ciudad)
        {
            if (string.IsNullOrWhiteSpace(ciudad)) return null;

            // Crear clave de caché para la ciudad
            var cacheKey = $"clima:{ciudad.ToLowerInvariant()}";
            var cacheExpiration = TimeSpan.FromMinutes(
                _configuration.GetValue<int>("Redis:CacheExpirationMinutes:Clima", 120));

            // Intentar obtener del caché o crear
            return await _cache.GetOrCreateAsync(cacheKey, async () =>
            {
                var info = await GetWeatherAsync(ciudad);
                if (info == null) return null!;

                var temperatura = info.Main?.Temp ?? 0.0;
                var descripcion = info.Weather != null && info.Weather.Count > 0
                    ? info.Weather[0].Description
                    : string.Empty;

                return new ClimaDto
                {
                    Temperatura = temperatura,
                    Descripcion = descripcion
                };
            }, cacheExpiration);
        }

        // DTO simple que espera el controlador (Temperatura, Descripcion)
        public class ClimaDto
        {
            public double Temperatura { get; set; }
            public string Descripcion { get; set; } = string.Empty;
        }
    }

    // DTOs para deserializar la respuesta de OpenWeather
    public class OpenWeatherResponse
    {
        public Coord? Coord { get; set; }
        public Main? Main { get; set; }
        public List<Weather>? Weather { get; set; }
        public string? Name { get; set; }
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
        public string? Description { get; set; }
        public string? Icon { get; set; }
    }

    // 🔹 Modelo simplificado para la vista/controlador
    public class WeatherResult
    {
        public string? Ciudad { get; set; }
        public double Temperatura { get; set; }
        public string? Descripcion { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}