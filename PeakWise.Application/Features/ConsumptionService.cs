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
    }
}
