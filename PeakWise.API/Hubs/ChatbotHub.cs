using Microsoft.AspNetCore.SignalR;
using PeakWise.Application.ExternalServices.Services;
using System.Security.Claims;

namespace PeakWise.API.Hubs
{
    public class ChatbotHub(ISmartAssistantService chatWithGemeniAsChatbot) : Hub
    {
        public async Task sendmessagetogemeni(string prompt)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var response = await chatWithGemeniAsChatbot.ChatWithGemeniAsChatbotWithSessionAsync(prompt, userId, default);
            await Clients.User(userId).SendAsync("receivefromchatbot", response);
        }
    }
}
