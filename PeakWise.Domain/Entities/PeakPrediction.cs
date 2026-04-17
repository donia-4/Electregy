using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Domain.Entities
{
    public class PeakPrediction
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int PeakHour { get; set; }
        public double ExpectedWatts { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
