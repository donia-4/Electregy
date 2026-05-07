using Microsoft.EntityFrameworkCore;
using PeakWise.Domain.Entities;
using PeakWise.Domain.Entities.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.Interfaces
{
    public interface IAppDbContext
    {
        public DbSet<DailyConsumption> DailyConsumptions { get;}
        public DbSet<UserRefreshToken> UserRefreshTokens { get;}
        public DbSet<Device> Devices { get; }
        public DbSet<Readings> Readings { get; }
        public DbSet<Alert> Alerts { get; }
        public DbSet<Recommendation> Recommendations { get; }
        public DbSet<PeakPrediction> PeakPredictions { get; }
        public DbSet<ChatSession> ChatMessages { get; }
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
