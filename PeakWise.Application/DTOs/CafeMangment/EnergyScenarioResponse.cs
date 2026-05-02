using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.CafeMangment
{
    public class EnergyScenarioResponse
    {
        public decimal Base_Energy { get; set; }

        public List<ScenarioDto> Scenarios { get; set; } = new();

        public string Best_Action { get; set; } = string.Empty;

        public decimal Best_Energy { get; set; }

        public decimal Best_Savings { get; set; }
    }
}
