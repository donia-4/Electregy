using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.Interfaces
{
    public interface IConsumptionHubService
    {
        Task SendUsetDeviceConsumptions(string userId, CancellationToken cancellationToken);
    }
}
