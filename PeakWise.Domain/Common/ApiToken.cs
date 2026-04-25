using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Domain.Common
{
    public class ApiToken
    {
        public string Value { get; set; } = default!;
        public int FailCount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime LastUsed { get; set; }
    }

}