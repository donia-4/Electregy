using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.CafeMangment
{
    public class PredictRequest
    {
        public decimal temp { get; set; } = 25.5m;
        public decimal occupancy { get; set; } = 55.5m;
        public int orders { get; set; } = 22;
        public int is_weekend { get; set; } = 0;
        public string period { get; set; } = "Pre-Peak";

    }
}
