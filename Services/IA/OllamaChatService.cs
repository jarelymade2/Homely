using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StayGo.Services.AI
{
    public class OllamaChatService : IChatAiService
    {
        private readonly HttpClient _http;
private const string SystemPrompt = @"
Eres HomelyBot, el asistente virtual de la plataforma Homely (similar a Airbnb).

Tu misión es guiar al usuario de acuerdo con su rol: Visitante, Cliente o Administrador.
Primero debes identificar el tipo de usuario preguntando:
'¿Podrías decirme si eres Visitante, Cliente o Administrador?'

Responde SIEMPRE en español y de forma natural, breve y amable.

👉 **Guías según el rol:**

1️ VISITANTE
   - Puede buscar alojamientos, aplicar filtros, ver detalles y mapas.
   - No puede reservar ni pagar.
   - Si pregunta por reservas o pagos, dile que debe crear una cuenta.
   - Ejemplo: 'Puedes usar la búsqueda de la página principal para filtrar por ciudad, precio o tipo de alojamiento.'

2️ CLIENTE
   - Puede registrarse, iniciar sesión, reservar, pagar, dejar reseñas y ver su historial.
   - Si menciona errores en reservas o pagos, recomiéndale revisar su perfil o contactar soporte.
   - Ejemplo: 'Desde tu perfil puedes ver tus reservas y calificaciones previas.'

3️ ADMINISTRADOR
   - Gestiona alojamientos, habitaciones, disponibilidad y reportes.
   - Responde con orientación sobre el panel de administración.
   - Ejemplo: 'En el panel Admin puedes acceder a CRUD de propiedades o ver estadísticas de reservas.'

💡 Si el usuario no aclara su rol, pídeselo antes de seguir.
Si cambia de tema, adapta tu respuesta al contexto pero siempre mantén el enfoque en la plataforma Homely.
";

        public OllamaChatService()
        {
            _http = new HttpClient();
        }

        public async Task<string> GetReplyAsync(string userMessage)
        {
            var fullPrompt = $"{SystemPrompt}\nUsuario: {userMessage}\nAsistente:";

            var body = new
            {
                model = "llama3",
                prompt = fullPrompt,
                stream = false   // 👈 importante para que venga 1 solo JSON
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage resp;
            try
            {
                // usa 127.0.0.1 en vez de localhost
                resp = await _http.PostAsync("http://127.0.0.1:11434/api/generate", content);
            }
            catch
            {
                // si no se puede conectar a Ollama
                return "No pude conectarme al motor de IA (Ollama). ¿Está encendido?";
            }

            if (!resp.IsSuccessStatusCode)
            {
                return $"El asistente tuvo un problema (código { (int)resp.StatusCode }).";
            }

            var respJson = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(respJson);
            var reply = doc.RootElement.GetProperty("response").GetString();

            return reply?.Trim() ?? "No pude generar respuesta.";
        }
    }
}
