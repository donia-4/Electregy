using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.CafeMangment
{
    public class SimulateResponse
    {
        public decimal Total_Baseline_Energy { get; set; }

        public decimal Total_Optimized_Energy { get; set; }

        public decimal Total_Savings_Kwh { get; set; }

        public decimal Saving_Percentage { get; set; }

        public List<PeriodDetailResponse> Period_Details { get; set; } = new();
    }
}
