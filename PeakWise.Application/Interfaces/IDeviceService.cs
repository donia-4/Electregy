using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeakWise.Application.DTOs.Devices;
using PeakWise.Shared.Responses;

namespace PeakWise.Application.Interfaces
{
    public interface IDeviceService
    {
        Task<Response<DeviceResponse>> CreateDeviceAsync(string userId, CreateDeviceRequest request, CancellationToken ct);
    }
}
