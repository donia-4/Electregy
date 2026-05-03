using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Domain.Entities
{
    public class DailyConsumption
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public double TotalKwh { get; set; }
        public double TotalCost { get; set; }
        public DateTime Date { get; set; }
    }
}
