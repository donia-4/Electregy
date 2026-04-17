using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Domain.Entities
{
    public class Recommendation
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public string Tip { get; set; }
        public double SavingEGP { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
