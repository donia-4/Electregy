using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeakWise.Domain.Enums;

namespace PeakWise.Application.DTOs.Devices
{
    public class DeviceResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DeviceType Type { get; set; }
        public double Watts { get; set; }
        public double HoursPerDay { get; set; }
    }
}
