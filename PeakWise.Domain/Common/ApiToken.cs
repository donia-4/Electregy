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
        public int FailCount { get; set; } = 0;
        public DateTime LastUsed { get; set; } = DateTime.MinValue;
        public DateTime? DisabledUntil { get; set; }
        public bool IsPermanentlyDisabled { get; set; } = false;
    }

}