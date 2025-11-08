using Microsoft.AspNetCore.Mvc;
using StayGo.Services.AI;

namespace StayGo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatApiController : ControllerBase
    {
        private readonly IChatAiService _chat;

        public ChatApiController(IChatAiService chat) => _chat = chat;

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest("Mensaje vacío");

            // Recordar rol en sesión (Visitante / Cliente / Administrador)
            var msg = req.Message;
            var role = HttpContext.Session.GetString("UserRole");

            if (msg.Contains("visitante", StringComparison.OrdinalIgnoreCase))
                role = "Visitante";
            else if (msg.Contains("cliente", StringComparison.OrdinalIgnoreCase))
                role = "Cliente";
            else if (msg.Contains("admin", StringComparison.OrdinalIgnoreCase) ||
                     msg.Contains("administrador", StringComparison.OrdinalIgnoreCase))
                role = "Administrador";

            if (!string.IsNullOrEmpty(role))
                HttpContext.Session.SetString("UserRole", role);

            var reply = await _chat.GetReplyAsync(msg, role);
            return Ok(new { reply });
        }
    }
}
