using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using PeakWise.Application.Interfaces;
using PeakWise.Domain.Entities;
using PeakWise.Domain.Enums;
using PeakWise.Infrastructure.Migrations;
using PeakWise.Infrastructure.Service;
using PeakWise.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatSession = PeakWise.Domain.Entities.ChatSession;

namespace PeakWise.Application.ExternalServices.Services.SmartAssistant
{
    public class SamrtAssistantService : ISmartAssistantService
    {

        private readonly IDeviceService _deviceService;
        private readonly TokenManager _tokenManager;
        //private readonly ResponseHandler responseHandler
        private readonly AppDbContext _context;
        public SamrtAssistantService(IDeviceService deviceService, AppDbContext context, TokenManager tokenManager)
        {
            _context = context;
            _deviceService = deviceService;
            _tokenManager = tokenManager;

        }


        #region Chatbot
        public async Task<string> ChatWithGemeniAsChatbotWithSessionAsync(string userInput, string userId, CancellationToken ct)
        {

            try
            {
                if (string.IsNullOrEmpty(userInput))
                    return "Please provide a valid input.";

                var prompt = await BuildPrompt(userId, "chatbot", userInput, ct);
                var response = await TryGenerateContentAsync(prompt);

                if (response.Item1 != HttpStatusCode.OK)
                    return response.Item2;
                else
                {

                    var chatMessage = new ChatSession
                    {
                        UserId = userId,
                        Message = response.Item2,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.ChatMessages.AddAsync(chatMessage, ct);
                    await _context.SaveChangesAsync(ct);
                    return response.Item2;
                }

            }
            catch (OperationCanceledException)
            {
                return "Request timeout or cancelled";
            }
            catch (HttpRequestException ex)
            {
                return $"Network error: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Unexpected error: {ex.Message}";
            }
        }
        #endregion

        #region Recommandation
        public async Task<string> ChatWithGemeniAsRecommandationWithSessionAsync(string userId, CancellationToken ct)
        {
            try
            {
                var prompt = await BuildPrompt(userId, "recommandation", ct: ct);
                var response = await TryGenerateContentAsync(prompt);

                if (response.Item1 != HttpStatusCode.OK)
                    return response.Item2;
                else
                {
                    return response.Item2;
                }

            }
            catch (OperationCanceledException)
            {
                return "Request timeout or cancelled";
            }
            catch (HttpRequestException ex)
            {
                return $"Network error: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Unexpected error: {ex.Message}";
            }
        }
        #endregion

        private async Task<string> GetDevicesJsonAsync(string userId, CancellationToken ct)
        {
            var devicesCount = await _context.Devices.CountAsync();
            var deviceInfo = await _deviceService.GetDevicesConsumptionSummaryAsync(userId, 1, devicesCount );

            if (deviceInfo?.Data?.Items == null || !deviceInfo.Data.Items.Any())
                return null;

            var items = deviceInfo.Data.Items.Select(d => new
            {
                DeviceType = Enum.TryParse<DeviceType>(d.DeviceType, true, out var deviceType) ? deviceType.ToString() : "Unknown",
                d.UsageKW,
                d.MonthCostEGP,
                d.TodayCostEGP,
                d.TodayHours,
                d.Name,
                d.TodayKwh
            });

            return JsonSerializer.Serialize(new { items }, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        private string BuildHistory(List<ChatSession> history)
        {
            var sb = new StringBuilder();

            foreach (var msg in history)
                sb.AppendLine(msg.Message);

            return sb.ToString();
        }
        private async Task<string> BuildPrompt(string userId, string type, string userInput = "", CancellationToken ct = default)
        {
            var devices = await GetDevicesJsonAsync(userId, ct);
            if (type == "chatbot")
            {
                var userMsg = new ChatSession
                {
                    UserId = userId,
                    Message = userInput,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.ChatMessages.AddAsync(userMsg);
                await _context.SaveChangesAsync(ct);

                var chatHistory = await _context.ChatMessages
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(5)
                    .ToListAsync(ct);
                var history = BuildHistory(chatHistory);

                return $"أنت مساعد ذكي لتطبيق Electregy.مهمتك:- مساعدة المستخدم في تقليل استهلاك الكهرباء" +
                         "- تحليل أجهزته ولازم تجاوب بالنسبة للمدخلات بتاعته" +
                         $"بيانات الأجهزة: {devices} سؤال المستخدم: {userInput}" +
                        $"محادثاتناالسابقة: {history}" +
                         "تعليمات: -لو السؤال عن الكهرباء أو التوفير → جاوب" +
                         "- لو خارج الموضوع تمامًا → ارفض" +
                         "- اجعل الإجابة 4 - 7 جمل واضحة ";
            }
            else
            {
                return $"أنت مستشار طاقة ذكي لتطبيق Electregy." +
                     $" الأجهزة:{devices}" +
                        "المطلوب:حلل استهلاك الكهرباء بناءً على الأجهزة فقط، وقدم توصيات لتقليل الفاتورة." +
                        "التعليمات:- حدد أعلى الأجهزة استهلاكاً- راقب استهلاكها في ساعات الذروة (6-11 مساءً)" +
                        "- اذكر توفير تقريبي بالجنيه المصري القواعد:- لا تسأل المستخدم أي أسئلة - لا تذكر أنك AI" +
                        "- استخدم البيانات فقط - لو لا توجد بيانات كافية قل: لا توجد بيانات كافية للتحليل الرد:" +
                        "4-7 جمل باللهجة المصرية، تحليل + توصية + توفير.";
            }
            throw new ArgumentException("Invalid prompt type");
        }

        #region Generate Content
        private async Task<(HttpStatusCode, string)> TryGenerateContentAsync(string prompt)
        {
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(75));
                GenerateContentResponse? response = null;
                //Console.WriteLine(prompt.Length);
                var result = await _tokenManager.ExecuteWithRetry(async token =>
                {
                    var googleAI = new GoogleAI(token);
                    //var model = googleAI.GenerativeModel("gemini-2.5-flash");
                    var model = googleAI.GenerativeModel("gemini-flash-lite-latest");

                    var response = await model.GenerateContent(prompt, cancellationToken: cts.Token);

                    return response?.Text;
                });
                if (string.IsNullOrWhiteSpace(result))
                {
                    return (HttpStatusCode.BadRequest, "Empty response from Gemini");
                }

                return (HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return (HttpStatusCode.BadRequest, $"Gemini Error: {ex.Message}");
            }
        }
        #endregion
    }
}