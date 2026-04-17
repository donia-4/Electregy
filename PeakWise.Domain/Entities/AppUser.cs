using Microsoft.AspNetCore.Identity;
using PeakWise.Domain.Entities;

public class AppUser : IdentityUser<string>
{
    public string FullName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Device> Devices { get; set; }
}