using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.Auth
{
    public class VerifyOtpRequest
    {
        public string UserId { get; set; }
        public string Otp { get; set; }
    }
}
