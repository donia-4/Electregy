using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeakWise.Domain.Enums;

namespace PeakWise.Domain.Entities
{
    public class Device
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public DeviceType Type { get; set; }
        public double Watts { get; set; }
        public double HoursPerDay { get; set; }

        public bool IsActive { get; set; } = true;

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Readings> Readings { get; set; }
        public ICollection<Alert> Alerts { get; set; }
    }
}
