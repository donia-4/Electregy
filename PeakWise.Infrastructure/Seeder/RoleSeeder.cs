using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Infrastructure.Seeder
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using PeakWise.Domain.Entities;

    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<Role> roleManager)
        {
            var roleExists = await roleManager.RoleExistsAsync("Consumer");

            if (!roleExists)
            {
                await roleManager.CreateAsync(new Role
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Consumer",
                    NormalizedName = "CONSUMER"
                });
            }
        }
    }
}
