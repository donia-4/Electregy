using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Infrastructure.Seeder
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public static class UserSeeder
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            var user = await userManager.FindByNameAsync("admin");

            if (user == null)
            {
                var adminUser = new AppUser()
                {
                    UserName = "admin",
                    Email = "iggugg122@gmail.com",
                    EmailConfirmed = true,
                    FullName = "System Admin"
                };

                await userManager.CreateAsync(adminUser, "Imshiyawalaa5*");

                await userManager.AddToRoleAsync(adminUser, "Consumer");
            }
        }
    }
}
