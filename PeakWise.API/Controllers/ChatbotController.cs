using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PeakWise.Application.ExternalServices.Services;
using System.Security.Claims;

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
           
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
            var response = await _chatWithGemeniAsChatbot.ChatWithGemeniAsChatbotAsync(userInput, userId, ct);
            return Ok(new { Response = response });
        }
    }
}
