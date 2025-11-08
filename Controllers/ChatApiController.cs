using Microsoft.AspNetCore.Mvc;
using StayGo.Services.AI;
<<<<<<< HEAD
=======
using System.Threading.Tasks;
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952

namespace StayGo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatApiController : ControllerBase
    {
<<<<<<< HEAD
        private readonly IChatAiService _chat;

        public ChatApiController(IChatAiService chat) => _chat = chat;
=======
        private readonly IChatAiService _chatService;

        public ChatApiController(IChatAiService chatService)
        {
            _chatService = chatService;
        }
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost]
<<<<<<< HEAD
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
=======
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Mensaje vacío");

            var reply = await _chatService.GetReplyAsync(request.Message);

>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
            return Ok(new { reply });
        }
    }
}
