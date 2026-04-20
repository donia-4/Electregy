using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.Auth
{
    public class RegisterResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
