using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PeakWise.Application.ExternalServices.Services;
using System.Security.Claims;

namespace PeakWise.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmartAssistantController : ControllerBase
    {
        private readonly ISmartAssistantService _chatWithGemeniAsChatbot;

        public SmartAssistantController(ISmartAssistantService chatWithGemeniAsChatbot)
        {
            _chatWithGemeniAsChatbot = chatWithGemeniAsChatbot;
        }

        [Authorize]
        [HttpPost("chatbot")]
        public async Task<IActionResult> Chat([FromBody] string userInput, CancellationToken ct)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var response = await _chatWithGemeniAsChatbot.ChatWithGemeniAsChatbotWithSessionAsync(userInput, userId, ct);
            return Ok(new { Response = response });
        }
        [Authorize]
        [HttpPost("recommand")]
        public async Task<IActionResult> GetRecommandation(CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var response = await _chatWithGemeniAsChatbot.ChatWithGemeniAsRecommandationWithSessionAsync(userId, ct);
            return Ok(new { Response = response });

        }
    }
}
