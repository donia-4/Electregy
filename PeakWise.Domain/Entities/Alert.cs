using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Domain.Entities
{
    public class Alert
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device Device { get; set; }

        public string Message { get; set; }
        public double PercentageOver { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? ResolvedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
