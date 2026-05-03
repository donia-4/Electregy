using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeakWise.Domain.Entities;

namespace PeakWise.Infrastructure.Configurations
{
        public class DailyConsumptionConfiguration : IEntityTypeConfiguration<DailyConsumption>
        {
            public void Configure(EntityTypeBuilder<DailyConsumption> builder)
            {
                builder.ToTable("DailyConsumptions");

                builder.HasKey(x => x.Id);

                builder.Property(x => x.UserId)
                    .IsRequired()
                    .HasMaxLength(450); 

                builder.Property(x => x.TotalKwh)
                    .IsRequired();

                builder.Property(x => x.TotalCost)
                    .IsRequired();

                builder.Property(x => x.Date)
                    .IsRequired();

                builder.HasIndex(x => new { x.UserId, x.Date })
                    .HasDatabaseName("IX_DailyConsumption_User_Date");
            }
        
    }
}
