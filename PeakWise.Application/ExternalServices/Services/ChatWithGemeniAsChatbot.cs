using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using PeakWise.Application.Interfaces;
using PeakWise.Domain.Entities;
using PeakWise.Domain.Enums;
using PeakWise.Infrastructure.Migrations;
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
        private static Queue<string> tokens = new(new[]
       {
            "AIzaSyDA_8oCDV-IbQ6N45WwhiwtbPV1fFn4VDw",
            "AIzaSyCKzNzt2laODA02kI-nfITvYwgdOJ2KN9M",
            "AIzaSyBpc4iHUUo2kJglj_VzU8QSKx4ZGweXmoI",
            "AIzaSyC9Aupr04oleAEp2HynBrV3E1R2CGz5k8g",
            "AIzaSyCzpFMuFg2yuuAoJ3D9oEU1HMi6MhYpB0w",
            "AIzaSyC-7EWaDnjIvEAGWIjP0GotZNH0fsA6HnU",
            "AIzaSyBv1GZHyVXX6H3sWnGqpET-anXMtELEOA8",
            "AIzaSyCQ8HJdG7htDfou4eny2GzG4osrOEeXtFg",
            "AIzaSyB7S3D-kw_syDfr5RhzsQNDKDAM69KXSo0",
            "AIzaSyAeqWSr-xYplwPNj-mwPZmBahDJxnIujkQ",
            "AIzaSyD3jjCneuuX_elOIyvWdlYahfhCbojGoQY",
        });
        private readonly IDeviceService _deviceService;
        private readonly GoogleAI _googleAI;
        private readonly GenerativeModel _generativeModel;
        //private readonly ResponseHandler responseHandler
        private readonly AppDbContext _context;
        public ChatWithGemeniAsChatbot(IDeviceService deviceService, AppDbContext context)
        {
            _context = context;
            _deviceService = deviceService;
            _googleAI = new GoogleAI(apiKey: tokens.First());
            _generativeModel = _googleAI.GenerativeModel("gemini-2.5-flash");
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
                    catch (TaskCanceledException ex)
                    {
                        return (HttpStatusCode.InternalServerError, "Check internet connection");
                    }
                    catch (OperationCanceledException ex)
                    {
                        return (HttpStatusCode.InternalServerError, "You have exceeded your limits");
                    }
                    catch (HttpRequestException ex)
                    {
                        return (HttpStatusCode.InternalServerError, "Check internet connection");
                    }

                }

                if (response == null)
                {
                    return (HttpStatusCode.BadRequest, "response is null");
                }

                return (HttpStatusCode.OK, response.Text);
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
            var finalPrompt = $"أنت مساعد ذكي لتطبيق مراقبة الكهرباء. \r\nالتعليمات:\r" +
                $"\n1. أجب فقط عن الكهرباء وتوفير الطاقة.\r" +
                $"\n2. إذا كان السؤال خارج التخصص رد بـ: 'لايمكنني الرد هذا السؤال خارج محتوي االابليكيشن'.\r" +
                $"\n3. كن مختصراً جداً.\r" +
                $"\n4. استخدم بيانات الأجهزة التالية لتقديم نصائح مخصصة:\r\n{devices}\r\n\r" +
                $"\nسياق المحادثة السابقة:\r\n{userHistory}\r\n\r\nالسؤال الحالي المطلوب الإجابة عليه الآن:\r\n{userInput}";

            return finalPrompt;
        }
    }
}