using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.Devices
{
    public class DeviceConsumptionSummaryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DeviceType { get; set; }

        public double UsageKW { get; set; } // Watts / 1000

        public double TodayHours { get; set; }

        public double TodayKwh { get; set; } // UsageKW * TodayHours
        public double TodayCostEGP { get; set; } // TodayKwh * 1.77
        public double MonthCostEGP { get; set; } // TodayKwh * 30 * 1.77
    }
}
