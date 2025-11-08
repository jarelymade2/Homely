using Microsoft.AspNetCore.Mvc;
using StayGo.Services.AI;
using System.Threading.Tasks;

namespace StayGo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatApiController : ControllerBase
    {
        private readonly IChatAiService _chatService;

        public ChatApiController(IChatAiService chatService)
        {
            _chatService = chatService;
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Mensaje vacío");

            var reply = await _chatService.GetReplyAsync(request.Message);

            return Ok(new { reply });
        }
    }
}
