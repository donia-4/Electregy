using Microsoft.AspNetCore.SignalR;
using PeakWise.Application.ExternalServices.Services;

namespace PeakWise.API.Hubs
{
    public class ChatbotHub(IChatWithGemeniAsChatbot chatWithGemeniAsChatbot) : Hub
    {
        public async Task sendmessagetogemeni(string prompt,string userId)
        {
            var response = await chatWithGemeniAsChatbot.ChatWithGemeniAsChatbotAsync(prompt, userId, default);
            await Clients.All.SendAsync("receivefromchatbot", response);
        }
    }
}
