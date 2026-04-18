using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PeakWise.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(AppUser applicationUser, string otp);
    }
}
