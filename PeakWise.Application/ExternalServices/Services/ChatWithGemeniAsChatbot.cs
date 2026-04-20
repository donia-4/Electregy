using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.ExternalServices.Services
{
    public class ChatWithGemeniAsChatbot : IChatWithGemeniAsChatbot
    {
       public static Queue<string> tokens = new(new[]
      {
            "AIzaSyDA_8oCDV-IbQ6N45WwhiwtbPV1fFn4VDw",
            "AIzaSyCKzNzt2laODA02kI-nfITvYwgdOJ2KN9M",
            "AIzaSyBpc4iHUUo2kJglj_VzU8QSKx4ZGweXmoI",
            "AIzaSyC9Aupr04oleAEp2HynBrV3E1R2CGz5k8g",
            "AIzaSyCzpFMuFg2yuuAoJ3D9oEU1HMi6MhYpB0w",
            "AIzaSyC-7EWaDnjIvEAGWIjP0GotZNH0fsA6HnU",
            "AIzaSyBv1GZHyVXX6H3sWnGqpET-anXMtELEOA8",
            //"AIzaSyD3jjCneuuX_elOIyvWdlYahfhCbojGoQY"
        });
        //private readonly Gemeni _gemeni;
        //private readonly List<string> Extensions = [".png", ".jpg", ".jpeg"];
        private readonly GoogleAI _googleAI;
        private readonly GenerativeModel _generativeModel;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private IConfiguration _config;
        private static ChatSession? _chatSession;
        public ChatWithGemeniAsChatbot(IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _webHostEnvironment = webHostEnvironment;
            _googleAI = new GoogleAI(apiKey: tokens.First());
            //  _chatSession= chatSession;
            _config = config;
            _generativeModel = _googleAI.GenerativeModel("gemini-2.5-flash");

        }
        public async Task<string> ChatWithGemeniAsChatbotAsync(string userInput, CancellationToken ct)
        {


            var response = await TryGenerateContentAsync($"{userInput}");
            var res = new
            {
                Data = response.Item2,
                StatusCode = 200,
                message = "Transelation Completed Successfully"
            };


            if (response.Item1 != HttpStatusCode.OK)
                return "You Have Exceeded Your Limits";
            else
                return response.Item2;




        }
        private async Task<(HttpStatusCode, string)> TryGenerateContentAsync(string prompt)
        {
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                GenerateContentResponse? response = null;

                foreach (var token in tokens.ToList())
                {
                    try
                    {
                        var googleAI = new GoogleAI(token);
                        var model = googleAI.GenerativeModel("gemini-2.5-flash");

                        response = await model.GenerateContent(prompt, cancellationToken: cts.Token);

                        if (response != null)
                        {
                            tokens.Dequeue();
                            tokens.Enqueue(token);
                            break;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (response == null)
                {
                    return (HttpStatusCode.BadRequest, "response is null");
                }

                var text = response.Text;

                return (HttpStatusCode.OK, text);
            }
            catch (Exception ex)
            {
                return (HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}
