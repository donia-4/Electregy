using PeakWise.Application.DTOs.CafeMangment;
using PeakWise.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakWise.Application.ExternalServices.Services.CafeMangment
{
    public interface ICafeMangmentService
    {
        //Task<Response<T>> PredictEnergyAsync<T,R>(R request, CancellationToken cancellationToken);
        //Task<Response<T>> EnergyPlanAsync<T,R>(R request,CancellationToken cancellationToken);
        //Task<Response<T>> ControlActionAsync<T,R>(R request,CancellationToken cancellationToken);
        //Task<Response<T>> SimulateAsync<T,R>(R request,CancellationToken cancellationToken);
        //Task<Response<T>> FullPlanAsync<T,R>(R request,CancellationToken cancellationToken);
        Task<Response<T>> CallAiModel<T, R>(string endPoint,R request, CancellationToken cancellationToken);
    }
}
