using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using PeakWise.Application.Interfaces;
using PeakWise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
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
        private readonly IDeviceService _deviceService;
        private readonly GoogleAI _googleAI;
        private readonly GenerativeModel _generativeModel;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private IConfiguration _config;
        private static ChatSession? _chatSession;
        public ChatWithGemeniAsChatbot(IWebHostEnvironment webHostEnvironment, IConfiguration config, IDeviceService deviceService)
        {
            _webHostEnvironment = webHostEnvironment;
            _deviceService = deviceService;
            _googleAI = new GoogleAI(apiKey: tokens.First());
            //  _chatSession= chatSession;
            _config = config;
            _generativeModel = _googleAI.GenerativeModel("gemini-2.5-flash");

        }
        public async Task<string> ChatWithGemeniAsChatbotAsync(string userInput, string userId="", CancellationToken ct = default)
        {
            if(string.IsNullOrEmpty(userInput))
                return "Please provide a valid input.";


            var deviceInfo = await _deviceService.GetUserDevicesAsync(userId, 1, 25, ct);
            if (deviceInfo?.Data?.Items == null || !deviceInfo.Data.Items.Any())
            {
                return "No devices found for this user";
            }
            var items = deviceInfo.Data.Items
                           .Select(d => new
                           {
                               name = d.Name,
                               type = ((DeviceType)int.Parse(d.Type)).ToString(),
                               watts = d.Watts,
                               hoursPerDay = d.HoursPerDay,
                               estimatedMonthlyCostEGP = d.EstimatedMonthlyCostEGP
                           }).ToList();

            var devices = JsonSerializer.Serialize(new { items }, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            //return devices;
            var prompt = $"{userInput} :انت لازم تجاوب في اطار الموضوع غير كدا قول لايمكنني الرد ودا سؤال المستخدم اللي هترد علي اساسه {devices} : انت دلوقتي شات بوت لتطبيق بيتم استخدامه عشان يراقب الكهرباء واستخداماتها مطلوب منك تساعد المستخدم في تساؤالاته عن الاجهزة بتاعته او في توفير الكهربا والتكلفة وتقترح ليه يقلل ايه من الاجهزة وعاوز رد مختصر ومش طويل وعندنا ودلوقتي عندنا مستخدم عنده المواصفات كالأتي";
            

            var response = await TryGenerateContentAsync(prompt);
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
