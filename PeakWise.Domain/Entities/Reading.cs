using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Domain.Entities
{
    public class Reading
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device Device { get; set; }

        public DateTime Timestamp { get; set; }
        public double WattsConsumed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
