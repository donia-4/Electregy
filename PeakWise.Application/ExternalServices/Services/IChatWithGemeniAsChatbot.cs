using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.ExternalServices.Services
{
    public interface IChatWithGemeniAsChatbot
    {
        Task<string> ChatWithGemeniAsChatbotAsync(string userInput,string userId,CancellationToken ct);
    }
}
