using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeakWise.Domain.Entities;

public class PeakPredictionConfiguration : IEntityTypeConfiguration<PeakPrediction>
{
    public void Configure(EntityTypeBuilder<PeakPrediction> builder)
    {
        builder.Property(x => x.PeakHour)
            .IsRequired();

        builder.Property(x => x.ExpectedWatts)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}