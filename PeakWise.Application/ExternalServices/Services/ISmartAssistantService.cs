using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.ExternalServices.Services
{
    public interface ISmartAssistantService
    {
        Task<string> ChatWithGemeniAsChatbotWithSessionAsync(string userInput, string userId, CancellationToken ct);
        public Task<string> ChatWithGemeniAsRecommandationWithSessionAsync(string userId, CancellationToken ct);
    }
}
