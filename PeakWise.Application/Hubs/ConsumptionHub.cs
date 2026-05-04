using Microsoft.AspNetCore.SignalR;
using PeakWise.Application.Interfaces;
using System.Security.Claims;

namespace PeakWise.API.Hubs
{
    public class ConsumptionHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await  base.OnConnectedAsync();
        }
    }
}
