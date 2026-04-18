using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Domain.Entities.Tokens
{
    public class UserRefreshToken
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string? Token { get; set; }
        public bool IsUsed { get; set; }
        public DateTime ExpiryDateUtc { get; set; }
        public virtual AppUser? User { get; set; }
    }
}
