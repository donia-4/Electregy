using FluentValidation;
using PeakWise.Application.DTOs.Devices;

public class CreateDeviceValidator : AbstractValidator<CreateDeviceRequest>
{
    public CreateDeviceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Device name is required");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid device type");

        RuleFor(x => x.Watts)
            .GreaterThan(0).WithMessage("Watts must be positive");

        RuleFor(x => x.HoursPerDay)
            .InclusiveBetween(0, 24)
            .WithMessage("Hours must be between 0 and 24");
    }
}