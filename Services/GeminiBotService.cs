using Mscc.GenerativeAI;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace StayGo.Services
{
    public class GeminiBotService
    {
        private readonly GenerativeModel _model;

        public GeminiBotService(IConfiguration configuration)
        {
            string apiKey = configuration["Gemini:ApiKey"]
                ?? throw new ArgumentNullException("Gemini:ApiKey no encontrada en appsettings.json");

            // ✅ Crear cliente Gemini
            var client = new GoogleAI(apiKey);

            // ✅ Modelo base
            _model = client.GenerativeModel("gemini-1.5-flash");
        }

        public async Task<string> GenerarRespuesta(string preguntaUsuario)
        {
            try
            {
                // ✅ En versiones recientes debe pasarse un array de strings
                var response = _model.Generate(new[] { preguntaUsuario });

                // El texto se encuentra en response.Candidates[0].Content.Parts
                return response?.Text ?? "(sin respuesta generada)";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al llamar a Gemini: {ex.Message}");
                return "Lo siento, ocurrió un problema al procesar tu solicitud.";
            }
        }
    }
}
