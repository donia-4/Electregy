using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeakWise.Domain.Entities;

public class ReadingConfiguration : IEntityTypeConfiguration<Readings>
{
    public void Configure(EntityTypeBuilder<Readings> builder)
    {
        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.Property(x => x.WattsConsumed)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}