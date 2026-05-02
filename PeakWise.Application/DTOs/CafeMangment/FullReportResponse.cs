using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.CafeMangment
{
    public class FullReportResponse
    {
        public ControlActionResponse Control_Actions { get; set; }
        public EnergyScenarioResponse Energy_plan { get; set; }
        public PredictResponse Energy_Prediction { get; set; }
    }
}
