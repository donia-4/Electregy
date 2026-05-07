using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PeakWise.Application.DTOs.Devices;
using PeakWise.Application.Interfaces;
using PeakWise.Domain.Entities;
using PeakWise.Shared.Pagination;
using PeakWise.Shared.Responses;

namespace PeakWise.Application.Features.Devices
{
    public class DeviceService : IDeviceService
    {
        private readonly IAppDbContext _context; 
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<DeviceService> _logger;

        // Average Egyptian electricity tariff rate (Can be moved to configuration) // later
        private const double TariffRateEGP = 1.77;

        public DeviceService(IAppDbContext context, ResponseHandler responseHandler, ILogger<DeviceService> logger)
        {
            _context = context;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<DeviceResponse>> CreateDeviceAsync(string userId, CreateDeviceRequest request)
        {
            try
            {
                var device = new Device
                {
                    Name = request.Name,
                    Type = request.Type,
                    Watts = request.Watts,
                    HoursPerDay = request.HoursPerDay,
                    UserId = userId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Set<Device>().AddAsync(device);
                await _context.SaveChangesAsync(default);

                var response = MapToResponse(device);
                return _responseHandler.Created(response, "Device created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating device for User: {UserId}", userId);
                return _responseHandler.InternalServerError<DeviceResponse>("An error occurred while adding the device.");
            }
        }

        public async Task<Response<PaginatedList<DeviceResponse>>> GetUserDevicesAsync(string userId, int pageNumber, int pageSize,CancellationToken token = default)
        {
            try
            {
                var query = _context.Set<Device>()
                    .Where(d => d.UserId == userId)
                    .OrderByDescending(d => d.CreatedAt)
                    .Select(d => new DeviceResponse
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Type = d.Type.ToString(),
                        Watts = d.Watts,
                        HoursPerDay = d.HoursPerDay,
                        EstimatedMonthlyCostEGP = ((d.Watts * d.HoursPerDay * 30) / 1000.0) * TariffRateEGP
                    });

                var paginatedDevices = await PaginatedList<DeviceResponse>.CreateAsync(query, pageNumber, pageSize);

                string message = paginatedDevices.Items.Any()
                    ? "Devices retrieved successfully."
                    : "No devices added yet.";

                return _responseHandler.Success(paginatedDevices, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving devices for User: {UserId}", userId);
                return _responseHandler.InternalServerError<PaginatedList<DeviceResponse>>("An error occurred while retrieving devices.");
            }
        }

        public async Task<Response<DeviceResponse>> UpdateDeviceAsync(string userId, UpdateDeviceRequest request)
        {
            // 1. Guard clause in case the entire request object is null
            if (request == null)
                return _responseHandler.BadRequest<DeviceResponse>("Update request cannot be null.");

            try
            {
                var device = await _context.Set<Device>().FirstOrDefaultAsync(d => d.Id == request.Id && d.UserId == userId);

                if (device == null)
                    return _responseHandler.NotFound<DeviceResponse>("Device not found or unauthorized.");

                // 2. Only update properties if they have a new value provided in the request
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    device.Name = request.Name;
                }

                if (request.Type.HasValue)
                {
                    device.Type = request.Type.Value;
                }

                if (request.Watts.HasValue)
                {
                    device.Watts = request.Watts.Value;
                }

                if (request.HoursPerDay.HasValue)
                {
                    device.HoursPerDay = request.HoursPerDay.Value;
                }

                _context.Set<Device>().Update(device);
                await _context.SaveChangesAsync(default);

                var response = MapToResponse(device);
                return _responseHandler.Success(response, "Device updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device {DeviceId} for User: {UserId}", request?.Id, userId);
                return _responseHandler.InternalServerError<DeviceResponse>("An error occurred while updating the device.");
            }
        }

        public async Task<Response<string>> DeleteDeviceAsync(string userId, int deviceId)
        {
            try
            {
                // Including Readings to ensure they are deleted as per Acceptance Criteria
                var device = await _context.Set<Device>()
                    .Include(d => d.Readings)
                    .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId);

                if (device == null)
                    return _responseHandler.NotFound<string>("Device not found or unauthorized.");

                // If EF Core Cascade Delete is on, removing the device removes readings. 
                // Explicit removal here ensures it happens even without cascade configuration.
                if (device.Readings != null && device.Readings.Any())
                {
                    _context.Set<Readings>().RemoveRange(device.Readings);
                }

                _context.Set<Device>().Remove(device);
                await _context.SaveChangesAsync(default);

                return _responseHandler.Success<string>(null, "Device and associated readings deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting device {DeviceId} for User: {UserId}", deviceId, userId);
                return _responseHandler.InternalServerError<string>("An error occurred while deleting the device.");
            }
        }

        public async Task<Response<PaginatedList<DeviceConsumptionSummaryResponse>>> GetDevicesConsumptionSummaryAsync(string userId, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.Set<Device>()
                    .Where(d => d.UserId == userId)
                    .OrderByDescending(d => d.CreatedAt)
                    .Select(d => new DeviceConsumptionSummaryResponse
                    {
                        Id = d.Id,
                        Name = d.Name,
                        DeviceType = d.Type.ToString(),
                        UsageKW = d.Watts / 1000.0,
                        TodayHours = d.HoursPerDay,
                        TodayKwh = (d.Watts * d.HoursPerDay) / 1000.0,
                        TodayCostEGP = ((d.Watts * d.HoursPerDay) / 1000.0) * TariffRateEGP,
                        MonthCostEGP = ((d.Watts * d.HoursPerDay * 30) / 1000.0) * TariffRateEGP
                    });

                var paginatedResult = await PaginatedList<DeviceConsumptionSummaryResponse>.CreateAsync(query, pageNumber, pageSize);
                return _responseHandler.Success(paginatedResult, "تم جلب ملخص الاستهلاك بنجاح.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error for User: {UserId}", userId);
                return _responseHandler.InternalServerError<PaginatedList<DeviceConsumptionSummaryResponse>>("حدث خطأ أثناء الحسابات.");
            }
        }

        public async Task<Response<DeviceConsumptionSummaryResponse>> GetDeviceConsumptionByIdAsync(string userId, int deviceId)
        {
            try
            {
                var deviceSummary = await _context.Set<Device>()
                    .Where(d => d.Id == deviceId && d.UserId == userId)
                    .Select(d => new DeviceConsumptionSummaryResponse
                    {
                        Id = d.Id,
                        Name = d.Name,
                        DeviceType = d.Type.ToString(),
                        UsageKW = d.Watts / 1000.0,
                        TodayHours = d.HoursPerDay,
                        TodayKwh = (d.Watts * d.HoursPerDay) / 1000.0,
                        TodayCostEGP = ((d.Watts * d.HoursPerDay) / 1000.0) * 1.77,
                        MonthCostEGP = ((d.Watts * d.HoursPerDay * 30) / 1000.0) * 1.77
                    }).FirstOrDefaultAsync();

                if (deviceSummary == null)
                    return _responseHandler.NotFound<DeviceConsumptionSummaryResponse>("الجهاز غير موجود.");

                return _responseHandler.Success<DeviceConsumptionSummaryResponse>(deviceSummary, "تم جلب ملخص استهلاك الجهاز بنجاح.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error for device {DeviceId}", deviceId);
                return _responseHandler.InternalServerError<DeviceConsumptionSummaryResponse>("حدث خطأ فني.");
            }
        }
        // Helper Method
        private DeviceResponse MapToResponse(Device device)
        {
            return new DeviceResponse
            {
                Id = device.Id,
                Name = device.Name,
                Type = device.Type.ToString(),
                Watts = device.Watts,
                HoursPerDay = device.HoursPerDay,
                EstimatedMonthlyCostEGP = ((device.Watts * device.HoursPerDay * 30) / 1000.0) * TariffRateEGP
            };
        }
    }
}