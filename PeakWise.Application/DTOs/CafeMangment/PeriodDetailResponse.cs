using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.CafeMangment
{
    public class PeriodDetailResponse
    {
        public string Period { get; set; } = string.Empty;

        public decimal Baseline_Energy { get; set; }

        public decimal Optimized_Energy { get; set; }

        public decimal Savings { get; set; }
    }
}
