using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeakWise.Application.DTOs.Devices;
using PeakWise.Application.Interfaces;
using PeakWise.Shared.Responses;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly CreateDeviceValidator _validator;
    private readonly ResponseHandler _responseHandler;

    public DevicesController(
        IDeviceService deviceService,
        CreateDeviceValidator validator,
        ResponseHandler responseHandler)
    {
        _deviceService = deviceService;
        _validator = validator;
        _responseHandler = responseHandler;
    }

    [HttpPost]
    //[Authorize(Roles = "Consumer")]
    public async Task<ActionResult<Response<DeviceResponse>>> CreateDevice([FromForm] CreateDeviceRequest request, CancellationToken ct)
    {
        if (request == null)
        {
            return StatusCode((int)_responseHandler.BadRequest<object>("Invalid body").StatusCode,
                _responseHandler.BadRequest<object>("Invalid body"));
        }

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));

            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var response = await _deviceService.CreateDeviceAsync(userId, request, ct);

        return StatusCode((int)response.StatusCode, response);
    }
}