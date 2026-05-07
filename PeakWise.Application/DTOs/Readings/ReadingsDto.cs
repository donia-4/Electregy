using PeakWise.Domain.Entities;
using PeakWise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.Readings
{
    public class ReadingsDto
    {
        public string DeviceName { get; set; }
        public DeviceType deviceType {  get; set; }
        public DateTime Timestamp { get; set; }
        public double WattsConsumed { get; set; }
        public string message { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public bool IsAbnormal { get; set; } = false;
    }
}
