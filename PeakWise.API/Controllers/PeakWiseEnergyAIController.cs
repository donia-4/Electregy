using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PeakWise.Application.DTOs.CafeMangment;
using PeakWise.Application.ExternalServices.Services.CafeMangment;
using PeakWise.Shared.Responses;

namespace PeakWise.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeakWiseEnergyAIController : ControllerBase
    {
        private readonly ICafeMangmentService _cafeMangmentService;
        public PeakWiseEnergyAIController(ICafeMangmentService cafeMangmentService)
        {
            _cafeMangmentService = cafeMangmentService;
        }

        [HttpPost("predict-energy")]
        [Authorize]
        public async Task<ActionResult<Response<PredictResponse>>> PredictEnergyAsync([FromBody] PredictRequest request, CancellationToken cancellationToken)
        {
            var response = await _cafeMangmentService.CallAiModel<PredictResponse, PredictRequest>("predict-energy", request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPost("energy-plan")]
        [Authorize]
        public async Task<ActionResult<Response<EnergyScenarioResponse>>> EnergyPlanAsync([FromBody] PredictRequest request, CancellationToken cancellationToken)
        {
            var response = await _cafeMangmentService.CallAiModel<EnergyScenarioResponse, PredictRequest>("energy-plan", request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPost("control-actions")]
        [Authorize]
        public async Task<ActionResult<Response<ControlActionResponse>>> ControlActionAsync([FromBody] PredictRequest request, CancellationToken cancellationToken)
        {
            var response = await _cafeMangmentService.CallAiModel<ControlActionResponse, PredictRequest>("control-actions", request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPost("simulate")]
        [Authorize]
        public async Task<ActionResult<Response<SimulateResponse>>> SimulateAsync([FromBody] SimulateRequest request, CancellationToken cancellationToken)
        {
            var response = await _cafeMangmentService.CallAiModel<SimulateResponse, SimulateRequest>("simulate", request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPost("full-report")]
        [Authorize]
        public async Task<ActionResult<Response<SimulateResponse>>> FullReportAsync([FromBody] PredictRequest request, CancellationToken cancellationToken)
        {
            var response = await _cafeMangmentService.CallAiModel<FullReportResponse, PredictRequest>("full-report", request, cancellationToken);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
