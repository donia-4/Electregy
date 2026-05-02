using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.CafeMangment
{
    public class PredictResponse
    {
        public string period { get; set; }
        public decimal predicted_energy { get; set; }
    }
}
