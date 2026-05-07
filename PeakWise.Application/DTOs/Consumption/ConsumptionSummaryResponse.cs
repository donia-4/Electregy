using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.Consumption
{
    public class ConsumptionSummaryResponse
    {
        public double TodayKwh { get; set; }
        public double TodayCostEGP { get; set; }
        public double MonthCostEGP { get; set; }
    }
}
