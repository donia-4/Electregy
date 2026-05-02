using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.CafeMangment
{
    public class SimulateRequest
    {
        public List<PredictRequest> Periods { get; set; } = new();

        public decimal Reduce_Occupancy_Pct { get; set; }

        public decimal Reduce_Orders_Pct { get; set; }
    }
}
