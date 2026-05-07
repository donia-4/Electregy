using PeakWise.Application.ExternalServices.Services.CafeMangment;
using PeakWise.Shared.Responses;
using System.Net.Http.Json;

namespace PeakWise.Infrastructure.ExternalServices.CafeMangment
{
    public class CafeMangmentService : ICafeMangmentService
    {
        private readonly ResponseHandler _responseHandler;
        private readonly HttpClient _httpClient;
        public CafeMangmentService(HttpClient httpClient, ResponseHandler responseHandler)
        {
            _httpClient = httpClient;
            _responseHandler = responseHandler;
        }

        public async Task<Response<T>> CallAiModel<T, R>(string endPoint, R request, CancellationToken cancellationToken)
        {
            var response = await SendRequestToAi<T, R>(endPoint, request, cancellationToken);
            return response;
        }

        //public async Task<Response<T>> ControlActionAsync<T, R>(R request, CancellationToken cancellationToken)
        //{
        //    var response = await SendRequestToAi<T, R>("control-actions", request, cancellationToken);
        //    return response;
        //}


        //public async Task<Response<T>> EnergyPlanAsync<T, R>(R request, CancellationToken cancellationToken)
        //{
        //    var response = await SendRequestToAi<T, R>("energy-plan", request, cancellationToken);
        //    return response;
        //}

        //public async Task<Response<T>> FullPlanAsync<T, R>(R request, CancellationToken cancellationToken)
        //{
        //    var response = await SendRequestToAi<T, R>("full-report", request, cancellationToken);
        //    return response;
        //}

        //public async Task<Response<T>> PredictEnergyAsync<T, R>(R request, CancellationToken cancellationToken)
        //{

        //    var response = await SendRequestToAi<T, R>("predict-energy", request, cancellationToken);
        //    return response;
        //}

        //public async Task<Response<T>> SimulateAsync<T, R>(R request, CancellationToken cancellationToken)
        //{
        //    var response = await SendRequestToAi<T, R>("simulate", request, cancellationToken);
        //    return response;
        //}

        private async Task<Response<T>> SendRequestToAi<T, R>(string end_Point, R request, CancellationToken cancellationToken)
        {
            var httpResponse = await _httpClient.PostAsJsonAsync(end_Point, request, cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                return _responseHandler.BadRequest<T>(
                   string.IsNullOrWhiteSpace(error)
                       ? "Failed To Fetch"
                       : error);
            }
            var data = await httpResponse.Content.ReadFromJsonAsync<T>();
            if (data is null)
                return _responseHandler.BadRequest<T>("Invalid response.");

            return _responseHandler.Success(data, "Data Retrived Successfully");
        }
    }
}
