using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeakWise.Application.DTOs.Devices;
using PeakWise.Application.Interfaces;
using PeakWise.Shared.Pagination;
using PeakWise.Shared.Responses;

namespace PeakWise.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All device operations require authentication
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly ResponseHandler _responseHandler;
        private readonly IValidator<CreateDeviceRequest> _createValidator;
        private readonly IValidator<UpdateDeviceRequest> _updateValidator;

        public DeviceController(
            IDeviceService deviceService,
            ResponseHandler responseHandler,
            IValidator<CreateDeviceRequest> createValidator,
            IValidator<UpdateDeviceRequest> updateValidator)
        {
            _deviceService = deviceService;
            _responseHandler = responseHandler;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        [HttpPost]
        public async Task<ActionResult<Response<DeviceResponse>>> Create([FromForm] CreateDeviceRequest request)
        {
            ValidationResult validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                    _responseHandler.BadRequest<object>(errors));
            }

            var response = await _deviceService.CreateDeviceAsync(GetUserId(), request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        public async Task<ActionResult<Response<PaginatedList<DeviceResponse>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _deviceService.GetUserDevicesAsync(GetUserId(), pageNumber, pageSize);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        public async Task<ActionResult<Response<DeviceResponse>>> Update([FromForm] UpdateDeviceRequest request)
        {
            ValidationResult validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                string errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                    _responseHandler.BadRequest<object>(errors));
            }

            var response = await _deviceService.UpdateDeviceAsync(GetUserId(), request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Response<string>>> Delete(int id)
        {
            var response = await _deviceService.DeleteDeviceAsync(GetUserId(), id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("consumption-summary")]
        public async Task<ActionResult<Response<PaginatedList<DeviceConsumptionSummaryResponse>>>> GetConsumptionSummary([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _deviceService.GetDevicesConsumptionSummaryAsync(GetUserId(), pageNumber, pageSize);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("{id}/consumption-summary")]
        public async Task<ActionResult<Response<DeviceConsumptionSummaryResponse>>> GetConsumptionSummaryById(int id)
        {
            var response = await _deviceService.GetDeviceConsumptionByIdAsync(GetUserId(), id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}