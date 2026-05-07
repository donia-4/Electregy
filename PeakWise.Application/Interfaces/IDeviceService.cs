using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeakWise.Application.DTOs.Devices;
using PeakWise.Shared.Pagination;
using PeakWise.Shared.Responses;

namespace PeakWise.Application.Interfaces
{
    public interface IDeviceService
    {
        Task<Response<DeviceResponse>> CreateDeviceAsync(string userId, CreateDeviceRequest request);
        Task<Response<PaginatedList<DeviceResponse>>> GetUserDevicesAsync(string userId, int pageNumber, int pageSize,CancellationToken token = default);
        Task<Response<DeviceResponse>> UpdateDeviceAsync(string userId, UpdateDeviceRequest request);
        Task<Response<string>> DeleteDeviceAsync(string userId, int deviceId);

        Task<Response<PaginatedList<DeviceConsumptionSummaryResponse>>> GetDevicesConsumptionSummaryAsync(string userId, int pageNumber, int pageSize);
        Task<Response<DeviceConsumptionSummaryResponse>> GetDeviceConsumptionByIdAsync(string userId, int deviceId);
    }
}
