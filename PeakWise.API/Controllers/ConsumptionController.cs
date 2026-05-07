using System.Security.Claims;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeakWise.Application;
using PeakWise.Application.DTOs.Consumption;
using PeakWise.Application.Interfaces;
using PeakWise.Infrastructure;
using PeakWise.Infrastructure.Common;
using PeakWise.Shared.Pagination;
using PeakWise.Shared.Responses;

namespace PeakWise.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsumptionController : ControllerBase
    {
        private readonly IConsumptionService _consumptionService;
        private readonly MockSimulatorState _simulatorState;

        public ConsumptionController(IConsumptionService consumptionService, MockSimulatorState simulatorState)
        {
            _consumptionService = consumptionService;
            _simulatorState = simulatorState;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        [HttpGet("summary")]
        public async Task<ActionResult<Response<ConsumptionSummaryResponse>>> GetSummary()
        {
            var response = await _consumptionService.GetUserSummaryAsync(GetUserId());
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("device/{deviceId}/readings")]
        public async Task<ActionResult<Response<PaginatedList<ReadingResponse>>>> GetReadings(int deviceId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var response = await _consumptionService.GetDeviceReadingsAsync(GetUserId(), deviceId, pageNumber, pageSize);
            return StatusCode((int)response.StatusCode, response);
        }

        // ========================================================
        // HACKATHON SECRET ENDPOINT: Trigger Anomaly for AI Demo
        // ========================================================
        [HttpPost("trigger-anomaly")]
        [AllowAnonymous] 
        public IActionResult TriggerAnomaly([FromQuery] bool activate)
        {
            _simulatorState.IsAnomalyActive = activate;
            string status = activate ? "ACTIVATED " : "DEACTIVATED ";
            return Ok(new { Message = $"Anomaly simulation is now {status}. Readings will spike!" });
        }

        [HttpPost("sync-my-chart")]
        public IActionResult SyncUserChart()
        {
            var userId = GetUserId(); 

            BackgroundJob.Enqueue<IConsumptionService>(service =>
                service.AggregateUserChartDataAsync(userId));

            return Ok(new { Message = "Your dashboard chart is being generated in the background!" });
        }

        [HttpGet("chart-data")]
        public async Task<ActionResult<Response<List<ChartDataResponse>>>> GetChartData()
        {
            var response = await _consumptionService.GetUserChartDataAsync(GetUserId()); 
            return StatusCode((int)response.StatusCode, response);
        }

    }
}