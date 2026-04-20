using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using PeakWise.Application.DTOs.Devices;

namespace PeakWise.Application.Validators
{
    public class UpdateDeviceValidator : AbstractValidator<UpdateDeviceRequest>
    {
        public UpdateDeviceValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Invalid device ID.");

            // Use When() to only validate if the user actually sent the field
            When(x => !string.IsNullOrWhiteSpace(x.Name), () => {
                RuleFor(x => x.Name).NotEmpty().WithMessage("Device name cannot be empty if provided.");
            });

            When(x => x.Type.HasValue, () => {
                RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid device type.");
            });

            When(x => x.Watts.HasValue, () => {
                RuleFor(x => x.Watts).GreaterThan(0).WithMessage("Watts must be a positive number.");
            });

            When(x => x.HoursPerDay.HasValue, () => {
                RuleFor(x => x.HoursPerDay).InclusiveBetween(0, 24).WithMessage("Hours must be between 0 and 24.");
            });
        }
    }
}
