using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.DTOs.Auth
{
    public class LoginResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string DisplayName { get; set; }
    }
}
