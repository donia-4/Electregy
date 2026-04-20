using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PeakWise.Application.ExternalServices.Services;

namespace PeakWise.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatWithGemeniAsChatbot _chatWithGemeniAsChatbot;

        public ChatbotController(IChatWithGemeniAsChatbot chatWithGemeniAsChatbot)
        {
            _chatWithGemeniAsChatbot = chatWithGemeniAsChatbot;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] string userInput, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return BadRequest("User input cannot be empty.");
            var response = await _chatWithGemeniAsChatbot.ChatWithGemeniAsChatbotAsync(userInput, ct);
            return Ok(new { Response = response });
        }
    }
}
