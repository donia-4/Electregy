using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PeakWise.Application.DTOs.Consumption;
using PeakWise.Application.Interfaces;
using PeakWise.Domain.Entities;
using PeakWise.Shared.Pagination;
using PeakWise.Shared.Responses;

namespace PeakWise.Application.Features
{
    public class ConsumptionService : IConsumptionService
    {
        private readonly AppDbContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<ConsumptionService> _logger;
        private const double TariffRateEGP = 1.77;

        public ConsumptionService(AppDbContext context, ResponseHandler responseHandler, ILogger<ConsumptionService> logger)
        {
            _context = context;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<ConsumptionSummaryResponse>> GetUserSummaryAsync(string userId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var startOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                // Get all readings for the user's devices
                var userReadings = _context.Set<Reading>()
                    .Include(r => r.Device)
                    .Where(r => r.Device.UserId == userId)
                    .AsNoTracking();

                // Calculate Today's Energy
                // Formula: Watts * (5 seconds / 3600) / 1000 = kWh per 5-second interval
                var todayWattsSum = await userReadings
                    .Where(r => r.Timestamp >= today)
                    .SumAsync(r => r.WattsConsumed);

                double todayKwh = (todayWattsSum * 5.0) / (3600.0 * 1000.0);

                // Calculate Month's Energy
                var monthWattsSum = await userReadings
                    .Where(r => r.Timestamp >= startOfMonth)
                    .SumAsync(r => r.WattsConsumed);

                double monthKwh = (monthWattsSum * 5.0) / (3600.0 * 1000.0);

                var response = new ConsumptionSummaryResponse
                {
                    TodayKwh = Math.Round(todayKwh, 4),
                    TodayCostEGP = Math.Round(todayKwh * TariffRateEGP, 2),
                    MonthCostEGP = Math.Round(monthKwh * TariffRateEGP, 2)
                };

                return _responseHandler.Success(response, "Consumption summary retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving summary for user: {UserId}", userId);
                return _responseHandler.InternalServerError<ConsumptionSummaryResponse>("An error occurred.");
            }
        }

        public async Task<Response<PaginatedList<ReadingResponse>>> GetDeviceReadingsAsync(string userId, int deviceId, int pageNumber, int pageSize)
        {
            try
            {
                // Verify device belongs to user
                bool ownsDevice = await _context.Set<Device>().AnyAsync(d => d.Id == deviceId && d.UserId == userId);
                if (!ownsDevice)
                    return _responseHandler.NotFound<PaginatedList<ReadingResponse>>("Device not found.");

                var query = _context.Set<Reading>()
                    .Where(r => r.DeviceId == deviceId)
                    .OrderByDescending(r => r.Timestamp)
                    .Select(r => new ReadingResponse
                    {
                        Id = r.Id,
                        Watts = r.WattsConsumed,
                        Timestamp = r.Timestamp
                    });

                var paginatedReadings = await PaginatedList<ReadingResponse>.CreateAsync(query, pageNumber, pageSize);

                return _responseHandler.Success(paginatedReadings, "Readings retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving readings for device: {DeviceId}", deviceId);
                return _responseHandler.InternalServerError<PaginatedList<ReadingResponse>>("An error occurred.");
            }
        }

        public async Task<Response<bool>> AggregateUserChartDataAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Starting chart data aggregation for user: {UserId}", userId);

                var last24Hours = DateTime.UtcNow.AddHours(-24);

                // جلب القراءات لليوزر ده بس
                var readings = await _context.Set<Reading>()
                    .Include(r => r.Device)
                    .Where(r => r.Device.UserId == userId && r.Timestamp >= last24Hours)
                    .AsNoTracking()
                    .ToListAsync();

                if (!readings.Any())
                {
                    return _responseHandler.Success(true, "No readings found for the last 24 hours.");
                }

                // تقسيم الداتا لمجموعات لكل ساعة (Logic التجميع)
                var hourlyData = readings
                    .GroupBy(r => new { r.Timestamp.Date, r.Timestamp.Hour })
                    .Select(g => new DailyConsumption
                    {
                        UserId = userId,
                        Date = g.Key.Date.AddHours(g.Key.Hour),
                        TotalKwh = Math.Round((g.Sum(r => r.WattsConsumed) * 5.0) / (3600.0 * 1000.0), 4),
                        TotalCost = Math.Round(((g.Sum(r => r.WattsConsumed) * 5.0) / (3600.0 * 1000.0)) * TariffRateEGP, 2)
                    }).ToList();

                // تنظيف البيانات القديمة لنفس الفترة واليوزر
                var existing = _context.Set<DailyConsumption>()
                    .Where(d => d.UserId == userId && d.Date >= last24Hours);

                _context.RemoveRange(existing);

                // إضافة البيانات الجديدة
                await _context.Set<DailyConsumption>().AddRangeAsync(hourlyData);
                await _context.SaveChangesAsync();

                return _responseHandler.Success(true, "Chart data aggregated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating chart data for user: {UserId}", userId);
                return _responseHandler.InternalServerError<bool>("An error occurred while processing chart data.");
            }
        }
        public async Task<Response<List<ChartDataResponse>>> GetUserChartDataAsync(string userId)
        {
            try
            {
                var last24Hours = DateTime.UtcNow.AddHours(-24);
                var data = await _context.Set<DailyConsumption>()
                    .Where(d => d.UserId == userId && d.Date >= last24Hours)
                    .OrderBy(d => d.Date)
                    .Select(d => new ChartDataResponse
                    {
                        Time = d.Date.ToString("HH:mm"),
                        Usage = d.TotalKwh,
                        Cost = d.TotalCost
                    }).ToListAsync();

                return _responseHandler.Success(data, "Chart data retrieved.");
            }
            catch (Exception ex)
            {
                return _responseHandler.InternalServerError<List<ChartDataResponse>>("Error fetching chart.");
            }
        }

        public async Task AggregateAllUsersChartDataAsync()
        {
            // هنجيب كل اليوزرز اللي ليهم أجهزة في السيستم
            var userIds = await _context.Set<Device>()
                .Select(d => d.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in userIds)
            {
                await AggregateUserChartDataAsync(userId);
            }
        }
    }

}
