using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StayGo.Services.AI
{
    public class OllamaChatService : IChatAiService
    {
        private readonly HttpClient _http = new HttpClient();

        // Prompt base (puedes ajustarlo)
        private const string SystemPrompt = @"
Eres HomelyBot, asistente de Homely (plataforma tipo Airbnb).
Responde SIEMPRE en español, breve y amable.
Primero identifica el rol del usuario (Visitante, Cliente o Administrador) y guíalo según corresponda.
Si no indica su rol, pídeselo con una sola pregunta clara antes de continuar.
";

        public async Task<string> GetReplyAsync(string userMessage, string? userRole = null)
        {
            var roleHint = string.IsNullOrWhiteSpace(userRole) ? "" : $"(El usuario es {userRole}). ";
            var fullPrompt = $"{SystemPrompt}\n{roleHint}Usuario: {userMessage}\nAsistente:";

            var body = new
            {
                model = "llama3",
                prompt = fullPrompt,
                stream = false
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage resp;
            try
            {
                // Usa 127.0.0.1 para evitar issues con localhost
                resp = await _http.PostAsync("http://127.0.0.1:11434/api/generate", content);
            }
            catch
            {
                return "No pude conectarme al motor de IA (Ollama). ¿Está encendido?";
            }

            if (!resp.IsSuccessStatusCode)
                return $"El asistente tuvo un problema (código {(int)resp.StatusCode}).";

            var respJson = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(respJson);
            var reply = doc.RootElement.GetProperty("response").GetString();
            return reply?.Trim() ?? "No pude generar respuesta.";
        }
    }
}
