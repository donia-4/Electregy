using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.Consumption
{
    public class ReadingResponse
    {
        public int Id { get; set; }
        public double Watts { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
