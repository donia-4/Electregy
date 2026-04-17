using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PeakWise.Domain.Enums;

namespace PeakWise.Application.DTOs.Devices
{
    public class CreateDeviceRequest
    {
        public string Name { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DeviceType Type { get; set; }
        public double Watts { get; set; }
        public double HoursPerDay { get; set; }
    }
}
