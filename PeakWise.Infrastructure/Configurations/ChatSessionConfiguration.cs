using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeakWise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Infrastructure.Configurations
{
    public class ChatSessionConfiguration : IEntityTypeConfiguration<PeakWise.Domain.Entities.ChatSession>
    {
        public void Configure(EntityTypeBuilder<ChatSession> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Message)
                .IsRequired()
                .HasMaxLength(450); 
        }
    }
}
