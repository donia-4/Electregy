using System.Text.Json.Serialization;
using PeakWise.Domain.Enums;

namespace PeakWise.Application.DTOs.Devices
{
    public class UpdateDeviceRequest
    {
        public int Id { get; set; } // Id is required to find the device

        public string? Name { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DeviceType? Type { get; set; }
        public double? Watts { get; set; }
        public double? HoursPerDay { get; set; }
    }
}