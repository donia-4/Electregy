using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeakWise.Application.DTOs.Devices;
using PeakWise.Application.Interfaces;
using PeakWise.Domain.Entities;
using PeakWise.Shared.Responses;

namespace PeakWise.Application.Features.Devices
{
    public class DeviceService : IDeviceService
    {
        private readonly AppDbContext _context;
        private readonly ResponseHandler _responseHandler;

        public DeviceService(AppDbContext context, ResponseHandler responseHandler)
        {
            _context = context;
            _responseHandler = responseHandler;
        }

        public async Task<Response<DeviceResponse>> CreateDeviceAsync(string userId, CreateDeviceRequest request, CancellationToken ct)
        {
            var device = new Device
            {
                Name = request.Name,
                Type = request.Type,
                Watts = request.Watts,
                HoursPerDay = request.HoursPerDay,
                UserId = userId
            };

            await _context.Devices.AddAsync(device, ct);
            await _context.SaveChangesAsync(ct);

            var response = new DeviceResponse
            {
                Id = device.Id,
                Name = device.Name,
                Type = device.Type,
                Watts = device.Watts,
                HoursPerDay = device.HoursPerDay
            };

            return _responseHandler.Success(response, "Device created successfully");
        }
    }
}
