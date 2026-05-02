using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.CafeMangment
{
    public class ScenarioDto
    {
        public string Action { get; set; } = string.Empty;

        public decimal Expected_Energy { get; set; }

        public decimal Savings { get; set; }
    }
}
