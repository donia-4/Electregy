using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PeakWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using PeakWise.API.Hubs;
using Microsoft.AspNetCore.Http;
using PeakWise.Application.DTOs.Readings;
using Humanizer;

namespace PeakWise.Application.Workers
{
    public class DataIngestionWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MockSimulatorState _simulatorState;
        private readonly ILogger<DataIngestionWorker> _logger;
        private readonly IHubContext<ConsumptionHub> _hubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public DataIngestionWorker(IServiceScopeFactory scopeFactory, MockSimulatorState simulatorState,
            ILogger<DataIngestionWorker> logger, IHubContext<ConsumptionHub> hubContext, IHttpContextAccessor httpContextAccessor)
        {
            _scopeFactory = scopeFactory;
            _simulatorState = simulatorState;
            _logger = logger;
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Data Ingestion Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // Use your AppDbContext

                    // Get all active devices to generate readings for them
                    var activeDevices = await dbContext.Set<Device>().Where(d => d.IsActive).ToListAsync(stoppingToken);
                    if (activeDevices.Any())
                    {
                        var random = new Random();
                        var readings = new List<Reading>();
                        //var readinsResponse = new List<ReadingsDto>();

                        foreach (var device in activeDevices)
                        {
                            // If Anomaly is active, shoot the watts up to 3000-4000W!
                            // Otherwise, normal behavior around the device's base watts.
                            double currentWatts = _simulatorState.IsAnomalyActive
                                ? random.Next(3000, 4500)
                                : random.Next((int)(device.Watts * 0.8), (int)(device.Watts * 1.2));

                            readings.Add(new Reading
                            {
                                DeviceId = device.Id,
                                WattsConsumed = currentWatts,
                                Timestamp = DateTime.UtcNow,
                                CreatedAt = DateTime.UtcNow
                            });

                            var response = new ReadingsDto()
                            {
                                DeviceName = device.Name,
                                AddedAt = device.CreatedAt,
                                deviceType = device.Type,
                                message = _simulatorState.IsAnomalyActive? "The unit is drawing unusual current levels":"The unit is drawing usual current levels",
                                Timestamp = DateTime.UtcNow,
                                WattsConsumed = currentWatts,
                                IsAbnormal = _simulatorState.IsAnomalyActive ? true : false
                            };
                            if(response.IsAbnormal) 
                            await _hubContext.Clients.Users(device.UserId).SendAsync("ReceivePowerAlert", response);
                            //readinsResponse.Add(response);
                          
                        }


                        await dbContext.Set<Reading>().AddRangeAsync(readings, stoppingToken);
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while generating mock readings.");
                }

                // Wait for 5 seconds before the next poll
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}