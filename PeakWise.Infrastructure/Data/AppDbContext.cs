using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PeakWise.Domain.Entities;
using PeakWise.Domain.Entities.Tokens;

public class AppDbContext : IdentityDbContext<AppUser, Role, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<DailyConsumption> DailyConsumptions { get; set; }
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Reading> Readings { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<Recommendation> Recommendations { get; set; }
    public DbSet<PeakPrediction> PeakPredictions { get; set; }
    public DbSet<ChatSession> ChatMessages { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Enum>()
            .HaveConversion<string>();
    }
}