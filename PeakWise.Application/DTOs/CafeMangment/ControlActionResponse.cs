using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.CafeMangment
{
    public class ControlActionResponse
    {
        public decimal Ac_Level { get; set; }

        public string Ac_Mode { get; set; } = string.Empty;

        public string Coffee_Machines { get; set; } = string.Empty;

        public string Staff_Preparation { get; set; } = string.Empty;

        public string Load_Distribution { get; set; } = string.Empty;
    }
}
