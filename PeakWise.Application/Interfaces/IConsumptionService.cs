using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeakWise.Application.DTOs.Consumption;
using PeakWise.Shared.Pagination;
using PeakWise.Shared.Responses;

namespace PeakWise.Application.Interfaces
{
    public interface IConsumptionService
    {
        Task<Response<ConsumptionSummaryResponse>> GetUserSummaryAsync(string userId);
        Task<Response<PaginatedList<ReadingResponse>>> GetDeviceReadingsAsync(string userId, int deviceId, int pageNumber, int pageSize);
        Task<Response<bool>> AggregateUserChartDataAsync(string userId);
        Task<Response<List<ChartDataResponse>>> GetUserChartDataAsync(string userId);
        Task AggregateAllUsersChartDataAsync();
    }
}
