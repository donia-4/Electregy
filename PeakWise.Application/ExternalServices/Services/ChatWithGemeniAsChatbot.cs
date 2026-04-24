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

namespace PeakWise.Application.ExternalServices.Services
{
    public class ChatWithGemeniAsChatbot : IChatWithGemeniAsChatbot
    {

        private readonly IDeviceService _deviceService;
        private readonly TokenManager _tokenManager;
        //private readonly ResponseHandler responseHandler
        private readonly AppDbContext _context;
        public ChatWithGemeniAsChatbot(IDeviceService deviceService, AppDbContext context, TokenManager tokenManager)
        {
            _context = context;
            _deviceService = deviceService;
            _tokenManager = tokenManager;

        }


        #region One session for all messages
        public async Task<string> ChatWithGemeniAsChatbotWithSessionAsync(string userInput, string userId, CancellationToken ct)
        {

            try
            {
                if (string.IsNullOrEmpty(userInput))
                    return "Please provide a valid input.";
                var userMsg = new ChatSession
                {
                    UserId = userId,
                    Message = userInput,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ChatMessages.Add(userMsg);
                await _context.SaveChangesAsync(ct);

                var history = await _context.ChatMessages
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(20)
                    .ToListAsync(ct);

                var prompt = await PreparePromptAsync(history, userInput, userId, ct);
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
        private async Task<(HttpStatusCode, string)> TryGenerateContentAsync(string prompt)
        {
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                GenerateContentResponse? response = null;

                var result = await _tokenManager.ExecuteWithRetry(async token =>
                {
                    var googleAI = new GoogleAI(token);
                    var model = googleAI.GenerativeModel("gemini-2.5-flash");

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
                return (HttpStatusCode.BadRequest, ex.Message);
            }
        }
        #endregion


        private async Task<string> PreparePromptAsync(List<ChatSession> history, string userInput, string userId, CancellationToken ct)
        {
            var userHistory = new StringBuilder();

            foreach (var msg in history)
            {
                userHistory.AppendLine(msg.Message);
            }

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
            var finalPrompt = $"أنت \"المساعد الذكي لـ Electregy\"،" +
                $" مستشار الطاقة الرقمي الأول في مصر المتخصص في مساعدة أصحاب الكافيهات، الأنشطة التجارية، والمنازل على إدارة استهلاك الكهرباء. مهمتك هي تحويل بيانات المستخدم إلى قرارات توفير ذكية." +
                $"نطاق معرفتك (محتوى التطبيق):تنبيهات أوقات الذروة:" +
                $" توعية المستخدم بأن الفترة من 6 مساءً وحتى 11 مساءً هي الأغلى والأكثر ضغطاً على الشبكة القومية." +
                $"محاكي السيناريوهات (What-If Simulator): القدرة على اقتراح تغييرات محددة (مثل تقليل ساعات التكييف أو تأجيل الأجهزة الثقيلة) وحساب التوفير المتوقع بالجنيه المصري." +
                $"لوحة تحكم التوفير: مقارنة الفواتير قبل وبعد استخدام التطبيق وإظهار \"إجمالي التوفير المتراكم\".الأثر القومي والاستدامة:" +
                $" ربط توفير الطاقة بتقليل انبعاثات الكربون ودعم أهداف التنمية المستدامة (SDGs) لمصر.سهولة الاستخدام: التأكيد على أن التطبيق يعمل بدون أي أجهزة إضافية (No Hardware) وببساطة عبر الموبايل فقط." +
                $"تعليمات الرد:تحليل البيانات: استخدم قائمة أجهزة المستخدم {devices} لتقديم نصائح رقمية دقيقة (مثلاً: \"جهاز X يستهلك Y، تقليل استخدامه سيوفر لك Z جنيه\")" +
                $".السياق: ارجع دائماً لـ {userHistory} لضمان استمرارية الحوار وكأنك تتابع معه تطور استهلاكه." +
                $"القيود: إذا كان سؤال المستخدم {userInput} خارج موضوعات (الكهرباء، الأجهزة، الفواتير، التوفير، أو ميزات Electregy)، رد حصراً بـ: " +
                $"\"لايمكنني الرد هذا السؤال خارج محتوي االابليكيشن\".طول الرد: اجعل إجابتك \"متوازنة\"؛ ليست كلمة واحدة وليست مقالاً." +
                $" قدم المعلومة المفيدة بوضوح (3-5 جمل مركزة).الشخصية: تفاعل كزميل خبير، محفز، ومهتم بمساعدة المستخدم على خفض تكاليفه وزيادة أرباحه.لماذا هذا البرومبت أفضل؟";

            return finalPrompt;
        }
    }
}