using Microsoft.AspNetCore.SignalR;
using PeakWise.Application.ExternalServices.Services;

namespace PeakWise.API.Hubs
{
    public class ChatbotHub(ISmartAssistantService chatWithGemeniAsChatbot) : Hub
    {
        public async Task sendmessagetogemeni(string prompt, string userId)
        {
            var response = await chatWithGemeniAsChatbot.ChatWithGemeniAsChatbotWithSessionAsync(prompt, userId, default);
            await Clients.User(userId).SendAsync("receivefromchatbot", response);
        }
    }
}
